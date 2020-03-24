using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Practices.ObjectBuilder2;
using VirtoCommerce.CatalogCsvImportModule.Core;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogCsvImportModule.Data.Core;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using SearchCriteria = VirtoCommerce.Domain.Catalog.Model.SearchCriteria;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvCatalogImporter : ICsvCatalogImporter
    {
        private readonly char[] _categoryDelimiters = { '/', '|', '\\', '>' };
        private readonly ICatalogService _catalogService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _productService;
        private readonly ISkuGenerator _skuGenerator;
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly IInventoryService _inventoryService;
        private readonly IFulfillmentCenterSearchService _fulfillmentCenterSearchService;
        private readonly IPropertyService _propertyService;
        private readonly ICatalogSearchService _searchService;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IProperyDictionaryItemSearchService _propDictItemSearchService;
        private readonly IProperyDictionaryItemService _propDictItemService;
        private readonly IStoreService _storeService;
        private readonly object _lockObject = new object();

        private readonly bool _createPropertyDictionatyValues;
        private List<Store> _stores = new List<Store>();

        public CsvCatalogImporter(ICatalogService catalogService, ICategoryService categoryService, IItemService productService, ISkuGenerator skuGenerator,
                                  IPricingService pricingService, IInventoryService inventoryService, IFulfillmentCenterSearchService fulfillmentCenterSearchService,
                                  IPropertyService propertyService, ICatalogSearchService searchService, Func<ICatalogRepository> catalogRepositoryFactory, IPricingSearchService pricingSearchService,
                                  ISettingsManager settingsManager, IStoreService storeService, IProperyDictionaryItemSearchService propDictItemSearchService, IProperyDictionaryItemService propDictItemService)
        {
            _catalogService = catalogService;
            _categoryService = categoryService;
            _productService = productService;
            _skuGenerator = skuGenerator;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _propertyService = propertyService;
            _searchService = searchService;
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _pricingSearchService = pricingSearchService;
            _storeService = storeService;
            _propDictItemSearchService = propDictItemSearchService;
            _propDictItemService = propDictItemService;
            _createPropertyDictionatyValues = settingsManager.GetValue("CsvCatalogImport.CreateDictionaryValues", false);
        }

        public void DoImport(Stream inputStream, CsvImportInfo importInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var csvProducts = new List<CsvProduct>();

            var progressInfo = new ExportImportProgressInfo
            {
                Description = "Reading products from csv..."
            };
            progressCallback(progressInfo);

            var encoding = DetectEncoding(inputStream);

            using (var reader = new CsvReader(new StreamReader(inputStream, encoding)))
            {
                reader.Configuration.Delimiter = importInfo.Configuration.Delimiter;
                reader.Configuration.RegisterClassMap(new CsvProductMap(importInfo.Configuration));
                reader.Configuration.MissingFieldFound = (strings, i, arg3) =>
                {
                    //do nothing
                };
                reader.Configuration.TrimOptions = TrimOptions.Trim;

                while (reader.Read())
                {
                    try
                    {
                        var csvProduct = reader.GetRecord<CsvProduct>();
                        csvProduct.Id = string.IsNullOrEmpty(csvProduct.Id) ? null : csvProduct.Id;
                        csvProducts.Add(csvProduct);
                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                        if (ex.Data.Contains("CsvHelper"))
                        {
                            error += ex.Data["CsvHelper"];
                        }
                        progressInfo.Errors.Add(error);
                        progressCallback(progressInfo);
                    }
                }
            }

            DoImport(csvProducts, importInfo, progressInfo, progressCallback);
        }

        private Encoding DetectEncoding(Stream stream)
        {
            var encoding = Encoding.UTF8;

            Ude.CharsetDetector cdet = new Ude.CharsetDetector();
            cdet.Feed(stream);
            cdet.DataEnd();
            if (cdet.Charset != null)
            {
                encoding = GetEncodingFromString(cdet.Charset);
            }

            stream.Position = 0;
            return encoding;
        }

        private Encoding GetEncodingFromString(string encoding)
        {
            try
            {
                return Encoding.GetEncoding(encoding);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        public void DoImport(List<CsvProduct> csvProducts, CsvImportInfo importInfo, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var catalog = _catalogService.GetById(importInfo.CatalogId);
            _stores.AddRange(_storeService.SearchStores(new Domain.Store.Model.SearchCriteria { Take = int.MaxValue }).Stores);

            var contunie = ImportAllowed(csvProducts, progressInfo, progressCallback);

            if (!contunie)
                return;

            csvProducts = MergeCsvProducts(csvProducts, catalog);

            MergeFromAlreadyExistProducts(csvProducts, catalog);

            SaveCategoryTree(catalog, csvProducts, progressInfo, progressCallback);

            LoadProductDependencies(csvProducts, catalog, progressInfo, progressCallback, importInfo);
            ResolvePropertyDictionaryItems(csvProducts, progressInfo, progressCallback);

            //take parentless prodcuts and save them first
            progressInfo.TotalCount = csvProducts.Count;

            var mainProcuts = csvProducts.Where(x => x.MainProduct == null).ToList();
            SaveProducts(mainProcuts, progressInfo, progressCallback);

            //prepare and save variations (needed to be able to save variation with SKU as MainProductId)
            var variations = csvProducts.Except(mainProcuts).ToList();
            variations.Where(x => x.MainProductId == null).ForEach(x => x.MainProductId = x.MainProduct.Id);
            SaveProducts(variations, progressInfo, progressCallback);
        }


        //Is it allowed to continue
        private bool ImportAllowed(List<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            progressInfo.Description = "Check product...";
            // Рere you can enter checks before import for example SeoAllowed(csvProducts) && SkuCkeck(csvProducts)
            return SeoAllowed(csvProducts, progressInfo, progressCallback);
        }

        private List<CsvProduct> MergeCsvProducts(List<CsvProduct> csvProducts, Catalog catalog)
        {
            var mergedCsvProducts = new List<CsvProduct>();

            var haveCodeProducts = csvProducts.Where(x => !string.IsNullOrEmpty(x.Code)).ToList();
            csvProducts = csvProducts.Except(haveCodeProducts).ToList();

            var groupedCsv = haveCodeProducts.GroupBy(x => new { x.Code });
            foreach (var group in groupedCsv)
            {
                mergedCsvProducts.Add(MergeCsvProductsGroup(group.ToList()));
            }

            var defaultLanguge = GetDefaultLanguage(catalog);
            MergeCsvProductComplexObjects(mergedCsvProducts, defaultLanguge);

            csvProducts.SelectMany(x => x.SeoInfos).Where(y => y.LanguageCode.IsNullOrEmpty()).ForEach(z => z.LanguageCode = defaultLanguge);
            csvProducts.SelectMany(x => x.Reviews).Where(y => y.LanguageCode.IsNullOrEmpty()).ForEach(x => x.LanguageCode = defaultLanguge);

            mergedCsvProducts.AddRange(csvProducts);
            return mergedCsvProducts;
        }

        private CsvProduct MergeCsvProductsGroup(List<CsvProduct> csvProducts)
        {
            var firstProduct = csvProducts.FirstOrDefault();
            if (firstProduct == null)
                return null;

            firstProduct.Reviews = csvProducts.SelectMany(x => x.Reviews).ToList();
            firstProduct.SeoInfos = csvProducts.SelectMany(x => x.SeoInfos).ToList();
            firstProduct.PropertyValues = csvProducts.SelectMany(x => x.PropertyValues).ToList();
            firstProduct.Prices = csvProducts.SelectMany(x => x.Prices).ToList();

            return firstProduct;
        }

        private void MergeCsvProductComplexObjects(List<CsvProduct> csvProducts, string defaultLanguge)
        {
            foreach (var csvProduct in csvProducts)
            {
                var reviews = csvProduct.Reviews.Where(x => x.Content != null).GroupBy(x => x.ReviewType).Select(g => g.FirstOrDefault()).ToList();
                reviews.Where(x => x.LanguageCode.IsNullOrEmpty()).ForEach(x => x.LanguageCode = defaultLanguge);
                csvProduct.Reviews = reviews;

                var seoInfos = csvProduct.SeoInfos.Where(x => x.SemanticUrl != null).GroupBy(x => x.SemanticUrl).Select(g => g.FirstOrDefault()).ToList();
                seoInfos.Where(x => x.LanguageCode.IsNullOrEmpty()).ForEach(x => x.LanguageCode = defaultLanguge);
                csvProduct.SeoInfos = seoInfos;

                csvProduct.PropertyValues = csvProduct.PropertyValues.GroupBy(x => new { x.PropertyName, x.Value }).Select(g => g.FirstOrDefault()).ToList();

                csvProduct.Prices = csvProduct.Prices.Where(x => x.EffectiveValue > 0).GroupBy(x => x.Currency).Select(g => g.FirstOrDefault()).ToList();
            }
        }

        private string GetDefaultLanguage(Catalog catalog)
        {
            return catalog.DefaultLanguage != null ? catalog.DefaultLanguage.LanguageCode : "en-US";
        }

        private void ResolvePropertyDictionaryItems(List<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var allDictPropertyIds = csvProducts.SelectMany(x => x.Properties).Where(x => x.Dictionary)
                                                .Select(x => x.Id).Distinct()
                                                .ToArray();

            var allDictItems = _propDictItemSearchService.Search(new PropertyDictionaryItemSearchCriteria
            {
                PropertyIds = allDictPropertyIds,
                Take = int.MaxValue
            }).Results;

            foreach (var dictPropValue in csvProducts.SelectMany(x => x.PropertyValues).Where(x => x.Property != null && x.Property.Dictionary && !string.IsNullOrEmpty(x.Value?.ToString())))
            {
                dictPropValue.Alias = dictPropValue.Value.ToString();
                var existDictItem = allDictItems.FirstOrDefault(x => x.PropertyId == dictPropValue.Property.Id && x.Alias.EqualsInvariant(dictPropValue.Alias));
                if (existDictItem == null)
                {
                    if (_createPropertyDictionatyValues)
                    {
                        existDictItem = new PropertyDictionaryItem
                        {
                            Alias = dictPropValue.Alias,
                            PropertyId = dictPropValue.Property.Id
                        };
                        allDictItems.Add(existDictItem);
                        _propDictItemService.SaveChanges(new[] { existDictItem });
                    }
                    else
                    {
                        progressInfo.Errors.Add($"The property dictionary '{dictPropValue.Alias}' not found in '{dictPropValue.Property.Name}' dictionary");
                        progressCallback(progressInfo);
                    }
                }
                dictPropValue.ValueId = existDictItem?.Id;
            }
        }

        /// <summary>
        /// Try to find (create if not) categories for products with Category.Path
        /// </summary>
        private void SaveCategoryTree(Catalog catalog, IEnumerable<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var cachedCategoryMap = new Dictionary<string, Category>();

            foreach (var csvProduct in csvProducts.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category.Path)))
            {
                var outline = "";
                var productCategoryNames = csvProduct.Category.Path.Split(_categoryDelimiters);
                string parentCategoryId = null;
                foreach (var categoryName in productCategoryNames)
                {
                    outline += "\\" + categoryName;
                    Category category;
                    if (!cachedCategoryMap.TryGetValue(outline, out category))
                    {
                        var searchCriteria = new SearchCriteria
                        {
                            CatalogId = catalog.Id,
                            CategoryId = parentCategoryId,
                            ResponseGroup = SearchResponseGroup.WithCategories
                        };
                        category = _searchService.Search(searchCriteria).Categories.FirstOrDefault(x => x.Name == categoryName);
                    }

                    if (category == null)
                    {
                        var code = categoryName.GenerateSlug();
                        if (string.IsNullOrEmpty(code))
                        {
                            code = Guid.NewGuid().ToString("N");
                        }
                        category = _categoryService.Create(new Category() { Name = categoryName, Code = code, CatalogId = catalog.Id, ParentId = parentCategoryId });
                        //Raise notification each notifyCategorySizeLimit category
                        var count = progressInfo.ProcessedCount;
                        progressInfo.Description = $"Creating categories: {++count} created";
                        progressCallback(progressInfo);
                    }
                    csvProduct.CategoryId = category.Id;
                    csvProduct.Category = category;
                    parentCategoryId = category.Id;
                    cachedCategoryMap[outline] = category;
                }
            }
        }

        private void SaveProducts(List<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var defaultFulfilmentCenter = _fulfillmentCenterSearchService.SearchCenters(new Domain.Inventory.Model.Search.FulfillmentCenterSearchCriteria { Take = 1 }).Results.FirstOrDefault();

            var totalProductsCount = csvProducts.Count();
            for (int i = 0; i < totalProductsCount; i += 10)
            {
                var products = csvProducts.Skip(i).Take(10).ToList();

                try
                {
                    _productService.Update(products.ToArray());

                    SaveProductInventories(products, defaultFulfilmentCenter);

                    SaveProductPrices(products);
                }
                catch (FluentValidation.ValidationException validationEx)
                {
                    lock (_lockObject)
                    {
                        foreach (var validationErrorGroup in validationEx.Errors.GroupBy(x => x.PropertyName))
                        {
                            string errorMessage = string.Join("; ", validationErrorGroup.Select(x => x.ErrorMessage));
                            progressInfo.Errors.Add(errorMessage);
                            progressCallback(progressInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (_lockObject)
                    {
                        progressInfo.Errors.Add(ex.ToString());
                        progressCallback(progressInfo);
                    }
                }
                finally
                {
                    lock (_lockObject)
                    {
                        //Raise notification
                        progressInfo.ProcessedCount += products.Count();
                        progressInfo.Description =
                            $"Saving products: {progressInfo.ProcessedCount} of {progressInfo.TotalCount} created";
                        progressCallback(progressInfo);
                    }
                }
            }
        }

        private void SaveProductInventories(IList<CsvProduct> products, FulfillmentCenter defaultFulfilmentCenter)
        {
            //Set productId for dependent objects
            foreach (var product in products)
            {
                if (defaultFulfilmentCenter != null || product.Inventory.FulfillmentCenterId != null)
                {
                    product.Inventory.ProductId = product.Id;
                    product.Inventory.FulfillmentCenterId = product.Inventory.FulfillmentCenterId ?? defaultFulfilmentCenter.Id;
                }
                else
                {
                    product.Inventory = null;
                }
            }
            var productIds = products.Select(x => x.Id).ToArray();
            var existInventories = _inventoryService.GetProductsInventoryInfos(productIds);
            var inventories = products.Where(x => x.Inventory != null).Select(x => x.Inventory).Where(x => !string.IsNullOrEmpty(x.ProductId)).ToArray();
            foreach (var inventory in inventories)
            {
                var exitsInventory = existInventories.FirstOrDefault(x => x.ProductId == inventory.ProductId && x.FulfillmentCenterId == inventory.FulfillmentCenterId);
                if (exitsInventory != null)
                {
                    inventory.ProductId = exitsInventory.ProductId;
                    inventory.FulfillmentCenterId = exitsInventory.FulfillmentCenterId;
                    inventory.AllowBackorder = exitsInventory.AllowBackorder;
                    inventory.AllowPreorder = exitsInventory.AllowPreorder;
                    inventory.BackorderAvailabilityDate = exitsInventory.BackorderAvailabilityDate;
                    inventory.BackorderQuantity = exitsInventory.BackorderQuantity;
                    inventory.InTransit = exitsInventory.InTransit;

                    inventory.InStockQuantity = inventory.InStockQuantity == 0 ? exitsInventory.InStockQuantity : inventory.InStockQuantity;
                }
            }
            _inventoryService.UpsertInventories(inventories);
        }

        private void SaveProductPrices(IList<CsvProduct> products)
        {
            // updating prices productid
            foreach (var product in products)
            {
                product.Prices.ForEach(p => p.ProductId = product.Id);
            }

            var prices = products.SelectMany(x => x.Prices).OfType<CsvPrice>().ToArray();
            //min quantity 0 is not allowed
            prices.Where(x => x.MinQuantity == 0).ForEach(x => x.MinQuantity = 1);

            //try update update prices by id
            var pricesWithIds = prices.Where(x => !string.IsNullOrEmpty(x.Id)).ToArray();
            var mergedPrices = GetMergedPriceById(pricesWithIds);

            //then update for products with PriceListId set
            var pricesWithPriceListIds = prices.Except(pricesWithIds).Where(x => !string.IsNullOrEmpty(x.PricelistId)).ToArray();
            mergedPrices.AddRange(GetMergedPriceByPriceList(pricesWithPriceListIds));

            //We do not have information about concrete price list id or price id and therefore select first product price then
            var restPrices = prices.Except(pricesWithIds).Except(pricesWithPriceListIds).ToArray();
            mergedPrices.AddRange(GetMergedPriceDefault(restPrices));

            _pricingService.SavePrices(mergedPrices.ToArray());
        }

        private IList<Price> GetMergedPriceById(IList<CsvPrice> pricesWithIds)
        {
            if (!pricesWithIds.Any())
                return new List<Price>();

            var result = new List<Price>();

            var pricesIds = pricesWithIds.Select(x => x.Id).ToArray();
            var existingPricesByIds = _pricingService.GetPricesById(pricesIds);
            foreach (var price in pricesWithIds)
            {
                var existPrice = existingPricesByIds.FirstOrDefault(x => x.Id == price.Id);
                if (existPrice != null)
                {
                    price.MergeFrom(existPrice);
                }
                result.Add(price);
            }

            return result;
        }

        private IList<Price> GetMergedPriceByPriceList(IList<CsvPrice> pricesWithPriceListIds)
        {
            if (!pricesWithPriceListIds.Any())
                return new List<Price>();

            var existPrices = new List<Price>();

            var dictionary = pricesWithPriceListIds.GroupBy(x => x.PricelistId).ToDictionary(g => g.Key, g => g.ToArray());
            foreach (var priceListId in dictionary.Keys)
            {
                var criteria = new Domain.Pricing.Model.Search.PricesSearchCriteria
                {
                    PriceListId = priceListId,
                    ProductIds = dictionary[priceListId].Select(x => x.ProductId).ToArray(),
                    Take = 1000
                };

                existPrices.AddRange(_pricingSearchService.SearchPrices(criteria).Results);
            }

            var result = new List<Price>();
            foreach (var price in pricesWithPriceListIds)
            {
                var existPrice = existPrices.FirstOrDefault(x => x.Currency.EqualsInvariant(price.Currency)
                    && x.ProductId.EqualsInvariant(price.ProductId) && x.PricelistId.EqualsInvariant(price.PricelistId));

                if (existPrice != null)
                {
                    price.MergeFrom(existPrice);
                }

                result.Add(price);
            }

            return result;
        }

        private IList<Price> GetMergedPriceDefault(IList<CsvPrice> restPrices)
        {
            if (!restPrices.Any())
                return new List<Price>();

            var criteria = new Domain.Pricing.Model.Search.PricesSearchCriteria
            {
                ProductIds = restPrices.Select(x => x.ProductId).ToArray(),
                Take = 1000
            };

            var result = new List<Price>();
            var existPrices = _pricingSearchService.SearchPrices(criteria).Results;
            foreach (var price in restPrices)
            {
                var existPrice = existPrices.FirstOrDefault(x => x.Currency.EqualsInvariant(price.Currency)
                    && x.ProductId.EqualsInvariant(price.ProductId));

                if (existPrice != null)
                {
                    price.MergeFrom(existPrice);
                }

                result.Add(price);
            }

            return result;
        }


        private void LoadProductDependencies(IEnumerable<CsvProduct> csvProducts, Catalog catalog, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CsvImportInfo importInfo)
        {
            var modifiedProperties = new List<Property>();
            var allCategoriesIds = csvProducts.Select(x => x.CategoryId).Distinct().ToArray();
            var categoriesMap = _categoryService.GetByIds(allCategoriesIds, CategoryResponseGroup.Full).ToDictionary(x => x.Id);

            foreach (var csvProduct in csvProducts)
            {
                csvProduct.Catalog = catalog;
                csvProduct.CatalogId = catalog.Id;
                if (csvProduct.CategoryId != null)
                {
                    csvProduct.Category = categoriesMap[csvProduct.CategoryId];
                }

                //Try to set parent relations
                //By id or code reference
                var parentProduct = csvProducts.FirstOrDefault(x => !string.IsNullOrEmpty(csvProduct.MainProductId) && (x.Id.EqualsInvariant(csvProduct.MainProductId) || x.Code.EqualsInvariant(csvProduct.MainProductId)));
                csvProduct.MainProduct = parentProduct;
                csvProduct.MainProductId = parentProduct != null ? parentProduct.Id : null;

                if (string.IsNullOrEmpty(csvProduct.Code))
                {
                    csvProduct.Code = _skuGenerator.GenerateSku(csvProduct);
                }
                //Properties inheritance
                csvProduct.Properties = (csvProduct.Category != null ? csvProduct.Category.Properties : csvProduct.Catalog.Properties).OrderBy(x => x.Name).ToList();

                foreach (var propertyValue in csvProduct.PropertyValues.ToArray())
                {
                    //Try to find property for product
                    propertyValue.Property = csvProduct.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(propertyValue.PropertyName));
                    if (propertyValue.Property != null)
                    {
                        propertyValue.ValueType = propertyValue.Property.ValueType;
                        propertyValue.PropertyId = propertyValue.Property.Id;
                        //Try to split the one value to multiple values for Multivalue properties
                        if (propertyValue.Property.Multivalue)
                        {
                            var multivalue = propertyValue.Value.ToString();
                            var chars = new[] { ",", importInfo.Configuration.Delimiter };
                            var values = multivalue.Split(chars, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim())
                                                   .Where(x => !string.IsNullOrEmpty(x))
                                                   .Distinct().ToArray();
                            propertyValue.Value = values.FirstOrDefault();
                            foreach (var value in values.Skip(1))
                            {
                                var newPropValue = propertyValue.Clone() as PropertyValue;
                                newPropValue.Value = value;
                                csvProduct.PropertyValues.Add(newPropValue);
                            }
                        }

                    }
                }
            }
        }

        //Merge importing products with already exist to prevent erasing already exist data, import should only update or create data
        private void MergeFromAlreadyExistProducts(IEnumerable<CsvProduct> csvProducts, Catalog catalog)
        {
            var transientProducts = csvProducts.Where(x => x.IsTransient()).ToArray();
            var nonTransientProducts = csvProducts.Where(x => !x.IsTransient()).ToArray();

            var alreadyExistProducts = new List<CatalogProduct>();
            //Load exist products
            for (int i = 0; i < nonTransientProducts.Count(); i += 50)
            {
                alreadyExistProducts.AddRange(_productService.GetByIds(nonTransientProducts.Skip(i).Take(50).Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge));
            }
            //Detect already exist product by Code
            var transientProductsCodes = transientProducts.Select(x => x.Code).Where(x => x != null).Distinct().ToArray();
            using (var repository = _catalogRepositoryFactory())
            {
                var products = repository.Items.Where(x => x.CatalogId == catalog.Id && transientProductsCodes.Contains(x.Code));
                var foundProducts = products.Select(x => new { x.Id, x.Code }).ToArray();
                for (int i = 0; i < foundProducts.Count(); i += 50)
                {
                    alreadyExistProducts.AddRange(_productService.GetByIds(foundProducts.Skip(i).Take(50).Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge));
                }
            }
            foreach (var csvProduct in csvProducts)
            {
                var existProduct = csvProduct.IsTransient() ? alreadyExistProducts.FirstOrDefault(x => x.Code.EqualsInvariant(csvProduct.Code)) : alreadyExistProducts.FirstOrDefault(x => x.Id == csvProduct.Id);
                if (existProduct != null)
                {
                    csvProduct.MergeFrom(existProduct);
                }
            }

        }

        #region Import allowed

        private bool SeoAllowed(List<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            bool isCompleted = true;

            foreach (var product in csvProducts)
            {
                if (!CorrectProduct(product))
                    isCompleted = false;
            }

            progressCallback(progressInfo);

            return isCompleted;

            bool CorrectProduct(CsvProduct product)
            {
                //check seoinfo storeif if specified
                if (!string.IsNullOrEmpty(product.SeoStore))
                {
                    var result = _stores.Any(x => x.Id == product.SeoStore);
                    if (!result)
                    {
                        progressInfo.Errors.Add($"No store with Id = {product.SeoStore}. Line number: {product.LineNumber}");
                    }

                    return result;
                }

                return true;
            }
        }

        #endregion
    }
}
