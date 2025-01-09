using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogCsvImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvCatalogExporter : ICsvCatalogExporter
    {
        private readonly IProductSearchService _productSearchService;
        private readonly IItemService _productService;
        private readonly IPricingEvaluatorService _pricingEvaluatorService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IInventorySearchService _inventorySearchService;
        private readonly IProductConfigurationSearchService _configurationSearchService;

        public CsvCatalogExporter(
            IProductSearchService productSearchService,
            IItemService productService,
            IPricingEvaluatorService pricingEvaluatorService,
            IInventorySearchService inventorySearchService,
            IBlobUrlResolver blobUrlResolver,
            IProductConfigurationSearchService configurationSearchService)
        {
            _productSearchService = productSearchService;
            _productService = productService;
            _pricingEvaluatorService = pricingEvaluatorService;
            _inventorySearchService = inventorySearchService;
            _blobUrlResolver = blobUrlResolver;
            _configurationSearchService = configurationSearchService;
        }

        public async Task DoExportAsync(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo
            {
                Description = "Counting products...",
                TotalCount = await GetProductsCount(exportInfo)
            };
            progressCallback(progressInfo);

            // It seems we need to read all the products twice. 
            progressInfo.Description = "Collecting product properties...";
            progressCallback(progressInfo);
            // First time to gather all dynamic properties to have full header
            await CollectPropertyCsvColumns(exportInfo, progressCallback, progressInfo);

            progressInfo.Description = "Export...";
            progressInfo.ProcessedCount = 0;
            progressCallback(progressInfo);

            // Second time: fetch and save to csv
            var streamWriter = new StreamWriter(outStream, Encoding.UTF8, 1024, true) { AutoFlush = true };

            var writerConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = exportInfo.Configuration.Delimiter
            };

            using (var csvWriter = new CsvWriter(streamWriter, writerConfig))
            {
                csvWriter.Context.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));

                csvWriter.WriteHeader<CsvProduct>();
                csvWriter.NextRecord();

                List<string> productsIds = null;

                if (!exportInfo.ProductIds.IsNullOrEmpty())
                {
                    // Just fetch for all the products
                    productsIds = new List<string>(exportInfo.ProductIds);
                    progressInfo.ProcessedCount += productsIds.Count;
                    await FetchThere(exportInfo, progressCallback, progressInfo, csvWriter, productsIds);
                }

                // Fetch page by page
                var currentPageNumber = 0;
                var pageSize = 50;
                var criteria = ProductSearchCriteriaFactory(exportInfo);
                var hasData = criteria != null;

                while (hasData)
                {
                    criteria.Skip = currentPageNumber * pageSize;
                    criteria.Take = pageSize;

                    var searchResult = await _productSearchService.SearchNoCloneAsync(criteria);
                    productsIds = searchResult.Results.Select(x => x.Id).ToList();
                    hasData = searchResult.Results.Any();
                    progressInfo.ProcessedCount += searchResult.Results.Count;
                    await FetchThere(exportInfo, progressCallback, progressInfo, csvWriter, productsIds);

                    currentPageNumber++;
                }

                progressInfo.Description = "Done.";
                progressCallback(progressInfo);
            }

            async Task FetchThere(CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback, ExportImportProgressInfo progressInfo, CsvWriter csvWriter, List<string> productsIds)
            {
                progressInfo.Description = string.Format("Exporting {0} of {1} products...", progressInfo.ProcessedCount, progressInfo.TotalCount);
                progressCallback(progressInfo);

                // Load products configurations
                var productConfigurations = (await _configurationSearchService.SearchNoCloneAsync(new ProductConfigurationSearchCriteria { ProductIds = productsIds })).Results;
                var productAsOptionIds = productConfigurations.SelectMany(c => c.Sections.SelectMany(s => s.Options.Select(o => o.ProductId)));

                productsIds.AddRange(productAsOptionIds);
                var products = await LoadProductsWithVariations(productsIds);
                var allProductIds = products.Select(x => x.Id).ToArray();

                // Load prices for products
                var priceEvalContext = new PriceEvaluationContext
                {
                    ProductIds = allProductIds,
                    PricelistIds = exportInfo.PriceListId == null ? null : [exportInfo.PriceListId],
                    Currency = exportInfo.Currency
                };
                var allProductPrices = (await _pricingEvaluatorService.EvaluateProductPricesAsync(priceEvalContext));

                // Load inventories
                var inventorySearchCriteria = new InventorySearchCriteria()
                {
                    ProductIds = allProductIds,
                    FulfillmentCenterIds = string.IsNullOrWhiteSpace(exportInfo.FulfilmentCenterId) ? Array.Empty<string>() : [exportInfo.FulfilmentCenterId],
                    Take = int.MaxValue,
                };
                var allProductInventories = (await _inventorySearchService.SearchInventoriesAsync(inventorySearchCriteria)).Results;

                // Convert to dict for faster search
                var pricesDict = allProductPrices.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());
                var inventoriesDict = allProductInventories.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());
                var usedConfigurations = new List<(ProductConfiguration, CatalogProduct)>();

                foreach (var productId in productsIds)
                {
                    try
                    {
                        var product = products.Single(x => x.Id == productId);
                        // We need to find the configuration where the product like an option. There may be duplicate products in options of different configurations
                        var configuration = productConfigurations.Where(x => !usedConfigurations.Contains((x, product))).FirstOrDefault(c => c.Sections.Any(s => s.Options.Any(o => o.ProductId == product.Id)));

                        if (configuration != null)
                        {
                            usedConfigurations.Add((configuration, product));
                        }

                        var csvProducts = MakeMultipleExportProducts(product, pricesDict, inventoriesDict, configuration);

                        csvWriter.WriteRecords(csvProducts);
                    }
                    catch (Exception ex)
                    {
                        progressInfo.Errors.Add(ex.ToString());
                        progressCallback(progressInfo);
                    }
                }

                // Need to rewrite with incode caching disable (when it arrived from techdebt)
                ItemCacheRegion.ExpireRegion();
                GC.Collect();
            }
        }

        private async Task CollectPropertyCsvColumns(CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback, ExportImportProgressInfo progressInfo)
        {
            List<string> productsIds = null;

            progressInfo.ProcessedCount = 0;

            if (!exportInfo.ProductIds.IsNullOrEmpty())
            {
                // Just fetch for all the products
                productsIds = new List<string>(exportInfo.ProductIds);
                progressInfo.ProcessedCount += productsIds.Count;
                await FetchThere(exportInfo, progressCallback, progressInfo, productsIds);
            }

            // Fetch page by page
            var currentPageNumber = 0;
            var pageSize = 50;
            var criteria = ProductSearchCriteriaFactory(exportInfo);
            var hasData = criteria != null;

            while (hasData)
            {
                criteria.Skip = currentPageNumber * pageSize;
                criteria.Take = pageSize;

                var searchResult = await _productSearchService.SearchNoCloneAsync(criteria);
                productsIds = searchResult.Results.Select(x => x.Id).ToList();
                hasData = searchResult.Results.Any();
                progressInfo.ProcessedCount += searchResult.Results.Count;

                await FetchThere(exportInfo, progressCallback, progressInfo, productsIds);

                currentPageNumber++;
            }

            async Task FetchThere(CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback, ExportImportProgressInfo progressInfo, List<string> productsIds)
            {
                progressInfo.Description = string.Format("Collecting props for {0} of {1} products...", progressInfo.ProcessedCount, progressInfo.TotalCount);
                progressCallback(progressInfo);

                var products = await LoadProductsWithVariations(productsIds);
                exportInfo.Configuration.PropertyCsvColumns = products.SelectMany(x => x.Properties).Select(x => x.Name).Union(exportInfo.Configuration.PropertyCsvColumns).Distinct().ToArray();

                // Need to rewrite with incode caching disable (when it arrived from techdebt)
                ItemCacheRegion.ExpireRegion();
                GC.Collect();
            }
        }

        private List<CsvProduct> MakeMultipleExportProducts(CatalogProduct product, Dictionary<string, Price> prices, Dictionary<string, InventoryInfo> inventories, ProductConfiguration productConfiguration)
        {
            var result = new List<CsvProduct>();

            prices.TryGetValue(product.Id, out var price);
            inventories.TryGetValue(product.Id, out var inventoryInfo);

            foreach (var seoInfo in product.SeoInfos.Any() ? product.SeoInfos : new List<SeoInfo>() { null })
            {
                var csvProduct = new CsvProduct(product, _blobUrlResolver, price, inventoryInfo, seoInfo, productConfiguration);

                result.Add(csvProduct);
            }

            return result;
        }

        private static ProductSearchCriteria ProductSearchCriteriaFactory(CsvExportInfo exportInfo)
        {
            ProductSearchCriteria result = null;

            if (!exportInfo.CategoryIds.IsNullOrEmpty())
            {
                result = new ProductSearchCriteria
                {
                    CatalogId = exportInfo.CatalogId,
                    CategoryIds = exportInfo.CategoryIds,
                    SearchInChildren = true,
                    SearchInVariations = false,
                };
            }

            if (exportInfo.CategoryIds.IsNullOrEmpty() && exportInfo.ProductIds.IsNullOrEmpty())
            {
                result = new ProductSearchCriteria
                {
                    CatalogId = exportInfo.CatalogId,
                    SearchInChildren = true,
                    SearchInVariations = false,
                };
            }

            return result;
        }

        private async Task<int> GetProductsCount(CsvExportInfo exportInfo)
        {
            var result = 0;
            if (!exportInfo.ProductIds.IsNullOrEmpty())
            {
                result += exportInfo.ProductIds.Length;
            }

            var criteria = ProductSearchCriteriaFactory(exportInfo);
            if (criteria != null)
            {
                criteria.Skip = 0;
                criteria.Take = 0;

                result += (await _productSearchService.SearchNoCloneAsync(criteria)).TotalCount;
            }

            return result;
        }

        private async Task<List<CatalogProduct>> LoadProductsWithVariations(List<string> productIds)
        {
            var result = new List<CatalogProduct>();
            var products = await _productService.GetAsync(productIds, ItemResponseGroup.ItemLarge.ToString());
            // Variations in products go without properties, only VariationProperties are included. Have to use GetByIdsAsync to receive all properties for variations.
            var variationsIds = products.SelectMany(product => product.Variations.Select(variation => variation.Id)).ToArray();
            var variations = await _productService.GetAsync(variationsIds, ItemResponseGroup.ItemLarge.ToString());

            foreach (var catalogProduct in products)
            {
                result.Add(catalogProduct);
                result.AddRange(variations.Where(x => x.MainProductId == catalogProduct.Id));
            }

            return result;
        }
    }
}
