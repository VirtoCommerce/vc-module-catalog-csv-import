using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using VirtoCommerce.CatalogCsvImportModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.CatalogCsvImportModule.Test
{
    public class ImporterTests
    {
        private Catalog _catalog = CreateCatalog();

        private List<Category> _categoriesInternal = new List<Category>();
        private List<CatalogProduct> _productsInternal = new List<CatalogProduct>();

        private List<Price> _pricesInternal = new List<Price>();
        private List<FulfillmentCenter> _fulfillmentCentersInternal = new List<FulfillmentCenter>();
        private List<InventoryInfo> _inventoryInfosInternal = new List<InventoryInfo>();

        private bool _createDictionatyValues = false;

        [Fact]
        public void DoImport_NewProductMultivalueDictionaryProperties_PropertyValuesCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "1, 3", ValueType = PropertyValueType.ShortText },
                new PropertyValue{ PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "2, 1", ValueType = PropertyValueType.ShortText}
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_MultivalueDictionary" && (string)x.Value == "1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_MultivalueDictionary" && (string) x.Value == "3"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_MultivalueDictionary" && (string) x.Value == "2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_MultivalueDictionary" && (string) x.Value == "1")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }

        [Fact]
        public void DoImport_NewProductDictionaryMultivaluePropertyWithNotExistingValue_ErrorIsPresent()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "NotExistingValue", ValueType = PropertyValueType.ShortText }
            };

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), exportInfo, info => { });

            //Assert
            Assert.True(exportInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_NewProductDictionaryMultivaluePropertyWithNewValue_NewPropertyValueCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "NewValue", ValueType = PropertyValueType.ShortText }
            };

            _createDictionatyValues = true;

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), exportInfo, info => { });

            //Assert
            var property = product.Properties.FirstOrDefault(x=>x.Name.Equals("CatalogProductProperty_1_MultivalueDictionary")).DictionaryValues.FirstOrDefault(y => y.Value.Equals("NewValue"));
            Assert.NotNull(property);
            Assert.True(!exportInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_UpdateProductDictionaryMultivalueProperties_PropertyValuesMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "2", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "3", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_MultivalueDictionary", Value = "3", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_MultivalueDictionary", Value = "1", ValueType = PropertyValueType.ShortText }
            };
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "2,3", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_MultivalueDictionary", Value = "2", ValueType = PropertyValueType.ShortText }
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_MultivalueDictionary" && (string) x.Value == "1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_MultivalueDictionary" && (string) x.Value == "2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_MultivalueDictionary" && (string) x.Value == "2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_MultivalueDictionary" && (string) x.Value == "3"),
                x => Assert.True(x.PropertyName == "TestCategory_ProductProperty_MultivalueDictionary" && (string) x.Value == "2")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductCategory_CategoryIsNotUpdated()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();
            product.Category = null;
            
            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Assert.True(product.Category.Id == existingProduct.Category.Id);
        }

        [Fact]
        public void DoImport_UpdateProductNameIsNull_NameIsNotUpdated()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();
            product.Name = null;

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Assert.True(product.Name == existingProduct.Name);
        }

        [Fact]
        public void DoImport_NewProductMultivalueProperties_PropertyValuesCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_2_Multivalue", Value = "TestValue1, TestValue2", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_1_Multivalue", Value = "TestValue2, TestValue1", ValueType = PropertyValueType.ShortText },
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue1")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductMultivalueProperties_PropertyValuesMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_1_Multivalue", Value = "TestValue1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_1_Multivalue", Value = "TestValue2", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_Multivalue", Value = "TestValue3", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_Multivalue", Value = "TestValue4", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_Multivalue", Value = "TestValue5", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_Multivalue", Value = "TestValue6", ValueType = PropertyValueType.ShortText }
            };
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_2_Multivalue", Value = "TestValue1, TestValue2", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_Multivalue", Value = "TestValue3", ValueType = PropertyValueType.ShortText }
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue2"),
                x => Assert.True(x.PropertyName == "TestCategory_ProductProperty_Multivalue" && (string) x.Value == "TestValue3")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }


        [Fact]
        public void DoImport_NewProductDictionaryProperties_PropertyValuesCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_1_Dictionary", Value = "1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_Dictionary", Value = "2", ValueType = PropertyValueType.ShortText },
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Dictionary" && (string) x.Value == "1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Dictionary" && (string) x.Value == "2")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }

        [Fact]
        public void DoImport_NewProductDictionaryPropertyWithNotExistingValue_ErrorIsPresent()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_Dictionary", Value = "NotExistingValue", ValueType = PropertyValueType.ShortText }
            };

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), exportInfo, info => { });

            //Assert
            Assert.True(exportInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_NewProductDictionaryPropertyWithNewValue_NewPropertyValueCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_Dictionary", Value = "NewValue", ValueType = PropertyValueType.ShortText }
            };

            _createDictionatyValues = true;

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), exportInfo, info => { });

            //Assert
            var property = product.Properties.FirstOrDefault(x => x.Name.Equals("CatalogProductProperty_1_Dictionary")).DictionaryValues.FirstOrDefault(y => y.Value.Equals("NewValue"));
            Assert.NotNull(property);
            Assert.True(!exportInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_NewProductProperties_PropertyValuesCreated()
        {
            //Arrange
            var target = GetImporter();

            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_1", Value = "1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2", Value = "2", ValueType = PropertyValueType.ShortText },
            };

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1" && (string) x.Value == "1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2" && (string) x.Value == "2")
            };
            Assert.Collection(product.PropertyValues, inspectors);
        }


        [Fact]
        public void DoImport_UpdateProductReviewIsEmpty_ReviewsNotClearedUp()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>();
            existingProduct.SeoInfos = new List<SeoInfo>
            {
                new SeoInfo()
                {
                    Id = "SeoInfo_test",
                    Name = "SeoInfo_test"
                }
            };
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Assert.True(product.SeoInfos.Count == 1);
            Assert.True(product.SeoInfos.First().Id == existingProduct.SeoInfos.First().Id);
        }

        [Fact]
        public void DoImport_UpdateProductSeoInfoIsEmpty_SeoInfosNotClearedUp()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>();
            existingProduct.Reviews = new List<EditorialReview>
            {
                new EditorialReview()
                {
                    Id = "EditorialReview_test",
                    Content = "EditorialReview_test"
                }
            };
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product = GetCsvProductBase();

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Assert.True(product.Reviews.Count == 1);
            Assert.True(product.Reviews.First().Id == existingProduct.Reviews.First().Id);
        }


        private CsvCatalogImporter GetImporter()
        {
            #region CatalogService

            var catalogService = new Mock<ICatalogService>();
            catalogService.Setup(x => x.GetById(It.IsAny<string>())).Returns(_catalog);

            #endregion

            #region CategoryService

            var categoryService = new Mock<ICategoryService>();
            categoryService.Setup(x => x.Create(It.IsAny<Category>()))
                .Returns((Category cat) =>
                {
                    cat.Id = Guid.NewGuid().ToString();
                    cat.Catalog = _catalog;
                    _categoriesInternal.Add(cat);
                    return cat;
                });

            categoryService.Setup(x => x.GetByIds(
                It.IsAny<string[]>(),
                It.Is<CategoryResponseGroup>(c => c == CategoryResponseGroup.Full),
                It.Is<string>(id => id == null)))
                .Returns((string[] ids, CategoryResponseGroup group, string catalogId) =>
                {
                    var result = ids.Select(id => _categoriesInternal.FirstOrDefault(x => x.Id == id));
                    result = result.Where(x => x != null).Select(x => x.Clone()).ToList();
                    foreach (var category in result)
                    {
                        if (category.Properties == null)
                            category.Properties = new List<Property>();

                        //emulate catalog property inheritance
                        category.Properties.AddRange(_catalog.Properties); 
                    }
                    return result.ToArray();
                });

            #endregion

            #region CatalogSearchService

            var catalogSearchService = new Mock<ICatalogSearchService>();
            catalogSearchService.Setup(x => x.Search(It.IsAny<SearchCriteria>())).Returns((SearchCriteria criteria) =>
                {
                    var result = new SearchResult();
                    var categories = _categoriesInternal.Where(x => x.CatalogId == criteria.CatalogId || x.Id == criteria.CategoryId).ToList();
                    var cloned = categories.Select(x => x.Clone()).ToList();
                    foreach (var category in cloned)
                    {
                        //search service doesn't return included properties
                        category.Properties = new List<Property>();
                    }
                    result.Categories = cloned;

                    return result;
                });

            #endregion

            #region ItemService

            var itemService = new Mock<IItemService>();
            itemService.Setup(x => x.GetByIds(
                It.IsAny<string[]>(),
                It.Is<ItemResponseGroup>(c => c == ItemResponseGroup.ItemLarge),
                It.Is<string>(id => id == null)))
                .Returns((string[] ids, ItemResponseGroup group, string catalogId) =>
                {
                    var result = _productsInternal.Where(x => ids.Contains(x.Id));
                    return result.ToArray();
                });

            itemService.Setup(x => x.Update(It.IsAny<CatalogProduct[]>())).Callback((CatalogProduct[] products) => { });

            #endregion

            #region repository mock

            var items = _productsInternal.Select(x => new ItemEntity { CatalogId = x.CatalogId, Id = x.Id, Code = x.Code }).ToList();
            var itemsQuerableMock = TestUtils.CreateQuerableMock(items);
            var catalogRepository = new Mock<ICatalogRepository>();
            catalogRepository.Setup(x => x.Items).Returns(itemsQuerableMock.Object);
            Func<ICatalogRepository> repositoryFactory = () => catalogRepository.Object;

            #endregion

            #region SkuGeneratorService

            var skuGeneratorService = new Mock<ISkuGenerator>();
            skuGeneratorService.Setup(x => x.GenerateSku(It.IsAny<CatalogProduct>())).Returns((CatalogProduct product) => Guid.NewGuid().GetHashCode().ToString());

            #endregion

            #region PricingService

            var pricingService = new Mock<IPricingService>();
            pricingService.Setup(x => x.SavePrices(It.IsAny<Price[]>())).Callback((Price[] prices) => { });

            #endregion

            #region InventoryService

            var inventoryService = new Mock<IInventoryService>();
            inventoryService.Setup(x => x.GetProductsInventoryInfos(It.IsAny<IEnumerable<string>>())).Returns(
                (IEnumerable<string> ids) =>
                {
                    var result = _inventoryInfosInternal.Where(x => ids.Contains(x.ProductId));
                    return result.ToList();
                });

            inventoryService.Setup(x => x.UpsertInventories(It.IsAny<IEnumerable<InventoryInfo>>())).Callback((IEnumerable<InventoryInfo> inventory) => { });

            #endregion

            #region CommerceService

            var commerceService = new Mock<ICommerceService>();
            commerceService.Setup(x => x.GetAllFulfillmentCenters()).Returns(() => _fulfillmentCentersInternal);

            #endregion

            #region PropertyService

            var propertyService = new Mock<IPropertyService>();
            propertyService.Setup(x => x.Update(It.IsAny<Property[]>())).Callback((Property[] properties) => { });

            #endregion

            #region PricingSearchService

            var pricingSearchService = new Mock<IPricingSearchService>();
            pricingSearchService.Setup(x => x.SearchPrices(It.IsAny<PricesSearchCriteria>()))
                .Returns((PricesSearchCriteria crietera) =>
                {
                    return new PricingSearchResult<Price>
                    {
                        Results = _pricesInternal.Where(x => crietera.ProductIds.Contains(x.ProductId)).ToArray()
                    };
                });

            #endregion

            #region settingsManager

            var settingsManager = new Mock<ISettingsManager>();
            settingsManager.Setup(x => x.GetValue(It.Is<string>(name => name == "CsvCatalogImport.CreateDictionaryValues"), false)).Returns((string name, bool defaultValue) => _createDictionatyValues);

            #endregion

            var target = new CsvCatalogImporter(catalogService.Object,
                categoryService.Object,
                itemService.Object,
                skuGeneratorService.Object,
                pricingService.Object,
                inventoryService.Object,
                commerceService.Object,
                propertyService.Object,
                catalogSearchService.Object,
                repositoryFactory,
                pricingSearchService.Object,
                settingsManager.Object
            );

            return target;
        }

        private static List<Property> CreateProductPropertiesInCategory(Category category, Catalog catalog)
        {
            var id = Guid.NewGuid().ToString();
            var multivalueDictionaryProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_MultivalueDictionary",
                Id = id,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                IsInherited = false,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = id },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = id },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = id }
                }
            };

            var multivalueProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_Multivalue",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = false,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var propId = Guid.NewGuid().ToString();
            var dictionaryProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_Dictionary",
                Id = propId,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = propId },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = propId },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = propId }
                }
            };

            var property = new Property
            {
                Name = $"{category.Name}_ProductProperty",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = false,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            return new List<Property> { multivalueDictionaryProperty, multivalueProperty, dictionaryProperty, property };
        }

        private static Catalog CreateCatalog()
        {
            var catalog = new Catalog { Name = "EmptyCatalogTest", Id = Guid.NewGuid().ToString(), Properties = new List<Property>() };

            var productPropertyId = Guid.NewGuid().ToString();
            var catalogProductProperty = new Property
            {
                Name = "CatalogProductProperty_1_MultivalueDictionary",
                Id = productPropertyId,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productPropertyId },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productPropertyId },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productPropertyId }
                }
            };

            var productProperty2Id = Guid.NewGuid().ToString();
            var catalogProductProperty2 = new Property
            {
                Name = "CatalogProductProperty_2_MultivalueDictionary",
                Id = productProperty2Id,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty2Id },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty2Id },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty2Id }
                }
            };

            var catalogProductProperty3 = new Property
            {
                Name = "CatalogProductProperty_1_Multivalue",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = false,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var catalogProductProperty4 = new Property
            {
                Name = "CatalogProductProperty_2_Multivalue",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = false,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var productProperty5Id = Guid.NewGuid().ToString();
            var catalogProductProperty5 = new Property
            {
                Name = "CatalogProductProperty_1_Dictionary",
                Id = productProperty5Id,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty5Id },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty5Id },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty5Id }
                }
            };

            var productProperty6Id = Guid.NewGuid().ToString();
            var catalogProductProperty6 = new Property
            {
                Name = "CatalogProductProperty_2_Dictionary",
                Id = productProperty6Id,
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                DictionaryValues = new List<PropertyDictionaryValue>
                {
                    new PropertyDictionaryValue { Alias = "1", Value = "1", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty6Id },
                    new PropertyDictionaryValue { Alias = "2", Value = "2", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty6Id },
                    new PropertyDictionaryValue { Alias = "3", Value = "3", LanguageCode = "en-US", Id = Guid.NewGuid().ToString(), PropertyId = productProperty6Id }
                }
            };

            var catalogProductProperty7 = new Property
            {
                Name = "CatalogProductProperty_1",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = false,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var catalogProductProperty8 = new Property
            {
                Name = "CatalogProductProperty_2",
                Id = Guid.NewGuid().ToString(),
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = false,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            catalog.Properties.Add(catalogProductProperty);
            catalog.Properties.Add(catalogProductProperty2);

            catalog.Properties.Add(catalogProductProperty3);
            catalog.Properties.Add(catalogProductProperty4);

            catalog.Properties.Add(catalogProductProperty5);
            catalog.Properties.Add(catalogProductProperty6);

            catalog.Properties.Add(catalogProductProperty7);
            catalog.Properties.Add(catalogProductProperty8);

            return catalog;
        }

        private static CsvProduct GetCsvProductBase()
        {
            var seoInfo = new SeoInfo { ObjectType = "CatalogProduct" };
            var review = new EditorialReview();
            return new CsvProduct
            {
                Category = new Category
                {
                    Parents = new Category[] { },
                    Path = "TestCategory"
                },
                Code = "TST1",
                Currency = "USD",
                EditorialReview = review,
                Reviews = new List<EditorialReview> { review },
                Id = "1",
                ListPrice = "100",
                Inventory = new InventoryInfo(),
                SeoInfo = seoInfo,
                SeoInfos = new List<SeoInfo> { seoInfo },
                Name = "TST1-TestCategory",
                Price = new Price(),
                Quantity = "0",
                Sku = "TST1",
                TrackInventory = true
            };
        }

        private Category CreateCategory(CsvProduct existingProduct)
        {
            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Catalog = _catalog,
                CatalogId = _catalog.Id,
                Name = existingProduct.Category.Path,
                Path = existingProduct.CategoryPath,
                Properties = new List<Property>()
            };
            category.Properties.AddRange(CreateProductPropertiesInCategory(category, _catalog));

            existingProduct.Category = category;
            existingProduct.CategoryId = category.Id;
            existingProduct.Catalog = _catalog;
            existingProduct.CatalogId = _catalog.Id;

            return category;
        }
    }
}