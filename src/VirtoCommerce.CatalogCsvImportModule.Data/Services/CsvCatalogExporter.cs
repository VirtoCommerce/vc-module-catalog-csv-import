using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogCsvImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvCatalogExporter : ICsvCatalogExporter
    {
        private readonly IProductSearchService _productSearchService;
        private readonly IItemService _productService;
        private readonly IPricingService _pricingService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IInventorySearchService _inventorySearchService;

        public CsvCatalogExporter(IProductSearchService productSearchService,
            IItemService productService,
            IPricingService pricingService,
            IInventorySearchService inventorySearchService,
            IBlobUrlResolver blobUrlResolver
            )
        {
            _productSearchService = productSearchService;
            _productService = productService;
            _pricingService = pricingService;
            _inventorySearchService = inventorySearchService;
            _blobUrlResolver = blobUrlResolver;
        }

        public async Task DoExportAsync(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback)
        {

            var progressInfo = new ExportImportProgressInfo
            {
                Description = "counting products...",
                TotalCount = await GetProductsCount(exportInfo)
            };

            // It seems we need to read all the products twice. 
            progressInfo.Description = "collecting product properties...";
            progressCallback(progressInfo);
            await CollectPropertyCsvColumns(exportInfo, progressCallback, progressInfo); // First time to gather all dynamic properties to have full header

            progressInfo.Description = "export...";
            progressInfo.ProcessedCount = 0;
            progressCallback(progressInfo);

            var criteria = ProductSearchCriteriaFactory(exportInfo);

            // Second time: fetch and save to csv
            var streamWriter = new StreamWriter(outStream, Encoding.UTF8, 1024, true) { AutoFlush = true };
            using (var csvWriter = new CsvWriter(streamWriter))
            {

                // Fetch page by page
                var CurrentPageNumber = 0;
                var PageSize = 50;
                var hasData = true;
                while (hasData)
                {
                    List<string> productsIds = null;
                    if (criteria == null)
                    { // Just fetch for all the products                    
                        productsIds = new List<string>(exportInfo.ProductIds);
                        hasData = false;
                    }
                    else
                    {
                        criteria.Skip = CurrentPageNumber * PageSize;
                        criteria.Take = PageSize;
                        var searchResult = await _productSearchService.SearchProductsAsync(criteria);
                        productsIds = searchResult.Results.Select(x => x.Id).ToList();
                        hasData = searchResult.Results.Any();
                        progressInfo.ProcessedCount += searchResult.Results.Count;
                        progressInfo.Description = string.Format("Processing {0} of {1} products...", progressInfo.ProcessedCount, progressInfo.TotalCount);
                        progressCallback(progressInfo);

                    }
                    var products = await LoadProducts(productsIds);

                    var allProductIds = products.Select(x => x.Id).ToArray();

                    //Load prices for products
                    progressInfo.Description = "loading prices...";
                    progressCallback(progressInfo);

                    var priceEvalContext = new PriceEvaluationContext
                    {
                        ProductIds = allProductIds,
                        PricelistIds = exportInfo.PriceListId == null ? null : new[] { exportInfo.PriceListId },
                        Currency = exportInfo.Currency
                    };
                    var allProductPrices = (await _pricingService.EvaluateProductPricesAsync(priceEvalContext)).ToList();

                    //Load inventories
                    progressInfo.Description = "loading inventory information...";
                    progressCallback(progressInfo);

                    var inventorySearchCriteria = new InventorySearchCriteria()
                    {
                        ProductIds = allProductIds,
                        FulfillmentCenterIds = string.IsNullOrWhiteSpace(exportInfo.FulfilmentCenterId) ? Array.Empty<string>() : new[] { exportInfo.FulfilmentCenterId },
                        Take = int.MaxValue,
                    };
                    var allProductInventories = (await _inventorySearchService.SearchInventoriesAsync(inventorySearchCriteria)).Results.ToList();

                    if (CurrentPageNumber == 0) //Write header
                    {
                        csvWriter.Configuration.Delimiter = exportInfo.Configuration.Delimiter;
                        csvWriter.Configuration.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));
                        
                        csvWriter.WriteHeader<CsvProduct>();
                        csvWriter.NextRecord();
                    }

                    //convert to dict for faster search
                    var pricesDict = allProductPrices.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());
                    var inventoriesDict = allProductInventories.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());

                    foreach (var product in products)
                    {
                        try
                        {
                            var csvProducts = MakeMultipleExportProducts(product, pricesDict, inventoriesDict);

                            csvWriter.WriteRecords(csvProducts);
                        }
                        catch (Exception ex)
                        {
                            progressInfo.Errors.Add(ex.ToString());
                            progressCallback(progressInfo);
                        }
                    }

                    CurrentPageNumber++;
                }
                progressInfo.Description = "Done.";
                progressCallback(progressInfo);
            }
        }

        private async Task CollectPropertyCsvColumns(CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback, ExportImportProgressInfo progressInfo)
        {
            progressInfo.ProcessedCount = 0;
            var CurrentPageNumber = 0;
            var PageSize = 50;

            var criteria = ProductSearchCriteriaFactory(exportInfo);
            // Fetch page by page
            var hasData = true;
            while (hasData)
            {
                List<string> productsIds = null;
                if (criteria == null)
                { // Just fetch for all the products                    
                    productsIds = new List<string>(exportInfo.ProductIds);
                    hasData = false;
                }
                else
                {
                    criteria.Skip = CurrentPageNumber * PageSize;
                    criteria.Take = PageSize;
                    CurrentPageNumber++;
                    var searchResult = await _productSearchService.SearchProductsAsync(criteria);
                    productsIds = searchResult.Results.Select(x => x.Id).ToList();
                    hasData = searchResult.Results.Any();

                    progressInfo.ProcessedCount += searchResult.Results.Count;
                    progressInfo.Description = string.Format("collecting props for {0} of {1} products...", progressInfo.ProcessedCount, progressInfo.TotalCount);
                    progressCallback(progressInfo);
                }
                var products = await LoadProducts(productsIds);
                exportInfo.Configuration.PropertyCsvColumns = products.SelectMany(x => x.Properties).Select(x => x.Name).Union(exportInfo.Configuration.PropertyCsvColumns).Distinct().ToArray();
            }
        }

        private List<CsvProduct> MakeMultipleExportProducts(CatalogProduct product, Dictionary<string, Price> prices, Dictionary<string, InventoryInfo> inventories)
        {
            var result = new List<CsvProduct>();

            prices.TryGetValue(product.Id, out var price);
            inventories.TryGetValue(product.Id, out var inventoryInfo);

            foreach (var seoInfo in product.SeoInfos.Any() ? product.SeoInfos : new List<SeoInfo>() { null })
            {
                var csvProduct = new CsvProduct(product, _blobUrlResolver, price, inventoryInfo, seoInfo);

                result.Add(csvProduct);
            }

            return result;
        }

        private ProductSearchCriteria ProductSearchCriteriaFactory(CsvExportInfo exportInfo)
        {
            ProductSearchCriteria result = null;
            if (exportInfo.CategoryIds != null && exportInfo.CategoryIds.Any())
            {
                result = new ProductSearchCriteria
                {
                    CatalogId = exportInfo.CatalogId,
                    CategoryIds = exportInfo.CategoryIds,
                    SearchInChildren = true,
                    SearchInVariations = false,
                };
            }
            if ((exportInfo.CategoryIds == null || !exportInfo.CategoryIds.Any()) && (exportInfo.ProductIds == null || !exportInfo.ProductIds.Any()))
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
            if (exportInfo.ProductIds != null && exportInfo.ProductIds.Length > 0)
            {
                return exportInfo.ProductIds.Count();
            }
            var criteria = ProductSearchCriteriaFactory(exportInfo);
            criteria.Skip = 0; criteria.Take = 0;

            return (await _productSearchService.SearchProductsAsync(criteria)).TotalCount;
        }

        private async Task<List<CatalogProduct>> LoadProducts(List<string> productIds)
        {
            var result = new List<CatalogProduct>();
            var products = await _productService.GetByIdsAsync(productIds.Distinct().ToArray(), ItemResponseGroup.ItemLarge.ToString());
            // Variations in products go without properties, only VariationProperties are included. Have to use GetByIdsAsync to receive all properties for variations.
            var variationsIds = products.SelectMany(product => product.Variations.Select(variation => variation.Id));
            var variations = await _productService.GetByIdsAsync(variationsIds.Distinct().ToArray(), ItemResponseGroup.ItemLarge.ToString());

            foreach (var catalogProduct in products)
            {
                result.Add(catalogProduct);
                result.AddRange(variations.Where(x => x.MainProductId == catalogProduct.Id));
            }

            return result;
        }
    }
}
