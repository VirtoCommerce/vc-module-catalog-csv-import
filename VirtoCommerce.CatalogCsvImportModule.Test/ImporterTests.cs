using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using VirtoCommerce.CatalogCsvImportModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using SearchCriteria = VirtoCommerce.Domain.Catalog.Model.SearchCriteria;
using SearchResult = VirtoCommerce.Domain.Catalog.Model.SearchResult;

namespace VirtoCommerce.CatalogCsvImportModule.Test
{
    public class ImporterTests
    {
        private Catalog _catalog = CreateCatalog();

        private List<Category> _categoriesInternal = new List<Category>();
        private List<CatalogProduct> _productsInternal = new List<CatalogProduct>();

        private List<Price> _pricesInternal = new List<Price>();
        private List<Domain.Inventory.Model.FulfillmentCenter> _fulfillmentCentersInternal = new List<Domain.Inventory.Model.FulfillmentCenter>();
        private List<InventoryInfo> _inventoryInfosInternal = new List<InventoryInfo>();

        private bool _createDictionatyValues = false;

        private List<CatalogProduct> _savedProducts;

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

            var progressInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, progressInfo, info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_1" && x.Alias == "1"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_2_MultivalueDictionary_2" && x.Alias == "2"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_3" && x.Alias == "3"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_2_MultivalueDictionary_1" && x.Alias == "1")
            };
            Assert.Collection(product.PropertyValues, inspectors);
            Assert.True(!progressInfo.Errors.Any());
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
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, exportInfo, info => { });

            //Assert
            Assert.True(exportInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_NewProductDictionaryMultivaluePropertyWithNewValue_NewDictPropertyItemCreated()
        {
            //Arrange
            var product = GetCsvProductBase();
            product.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue{ PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "NewValue", ValueType = PropertyValueType.ShortText }
            };

            _createDictionatyValues = true;

            var mockPropDictItemService = new Moq.Mock<IProperyDictionaryItemService>();
            var target = GetImporter(mockPropDictItemService.Object);

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, exportInfo, info => { });

            //Assert
            mockPropDictItemService.Verify(mock => mock.SaveChanges(It.Is<PropertyDictionaryItem[]>(dictItems => dictItems.Any(dictItem => dictItem.Alias == "NewValue"))), Times.Once());

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
            var progressInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, progressInfo, info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_1" && x.Alias == "1"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_2" && x.Alias == "2"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_2_MultivalueDictionary_2" && x.Alias == "2"),
                x => Assert.True(x.ValueId == "TestCategory_ProductProperty_MultivalueDictionary_2" && x.Alias == "2"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_2_MultivalueDictionary_3" && x.Alias == "3")
            };
            Assert.Collection(product.PropertyValues, inspectors);
            Assert.True(!progressInfo.Errors.Any());
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
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, new ExportImportProgressInfo(), info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue1"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_1_Multivalue" && (string) x.Value == "TestValue2"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue1"),
                x => Assert.True(x.PropertyName == "TestCategory_ProductProperty_Multivalue" && (string) x.Value == "TestValue3"),
                x => Assert.True(x.PropertyName == "CatalogProductProperty_2_Multivalue" && (string) x.Value == "TestValue2")
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

            var mockPropDictItemService = new Moq.Mock<IProperyDictionaryItemService>();
            var target = GetImporter(mockPropDictItemService.Object);


            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { product }, new CsvImportInfo(), exportInfo, info => { });

            //Assert
            mockPropDictItemService.Verify(mock => mock.SaveChanges(It.Is<PropertyDictionaryItem[]>(dictItems => dictItems.Any(dictItem => dictItem.Alias == "NewValue"))), Times.Once());
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
        public void DoImport_UpdateProductSeoInfoIsEmpty_SeoInfosNotClearedUp()
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
        public void DoImport_UpdateProductReviewIsEmpty_ReviewsNotClearedUp()
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

        [Fact]
        public void DoImport_UpdateProductTwoProductsWithSameCode_ProductsMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.Id = null;
            secondProduct.Id = null;

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Assert.True(_savedProducts.Count == 1);
        }

        [Fact]
        public void DoImport_TwoProductsSameCodeDifferentReviewTypes_ReviewsMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };
            existingProduct.Reviews.Clear();

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.EditorialReview.ReviewType = "FullReview";
            firstProduct.EditorialReview.Content = "Review Content 1";
            secondProduct.EditorialReview.ReviewType = "QuickReview";
            secondProduct.EditorialReview.Content = "Review Content 2";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<EditorialReview>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.Content == "Review Content 1"),
                x => Assert.True(x.LanguageCode == "en-US" && x.Content == "Review Content 2")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().Reviews, inspectors);
        }

        [Fact]
        public void DoImport_TwoProductsSameCodeSameReviewTypes_ReviewsMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };
            existingProduct.Reviews.Clear();

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.EditorialReview.Content = "Review Content 1";
            secondProduct.EditorialReview.Content = "Review Content 2";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<EditorialReview>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.Content == "Review Content 1")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().Reviews, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductTwoProductsSameCodeDifferentReviewTypes_ReviewsMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };
            existingProduct.Reviews = new List<EditorialReview>() { new EditorialReview() { Content = "Review Content 3", Id = "1", LanguageCode = "en-US", ReviewType = "QuickReview" } };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.EditorialReview.ReviewType = "FullReview";
            firstProduct.EditorialReview.Content = "Review Content 1";
            secondProduct.EditorialReview.ReviewType = "QuickReview";
            secondProduct.EditorialReview.Content = "Review Content 2";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<EditorialReview>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.Content == "Review Content 1" && x.ReviewType == "FullReview"),
                x => Assert.True(x.LanguageCode == "en-US" && x.Content == "Review Content 2" && x.ReviewType == "QuickReview")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().Reviews, inspectors);
        }

        [Fact]
        public void DoImport_TwoProductsSameCodeDifferentSeoInfo_SeoInfosMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.SeoInfos.Clear();

            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.SeoInfo.SemanticUrl = "SemanticsUrl1";
            secondProduct.SeoInfo.SemanticUrl = "SemanticsUrl2";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<SeoInfo>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl1"),
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl2")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().SeoInfos, inspectors);
        }

        [Fact]
        public void DoImport_TwoProductsSameCodeSameSeoInfo_SeoInfosMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.SeoInfos.Clear();

            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.SeoInfo.SemanticUrl = "SemanticsUrl1";
            secondProduct.SeoInfo.SemanticUrl = "SemanticsUrl1";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<SeoInfo>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl1")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().SeoInfos, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductTwoProductsSameCodeDifferentSeoInfo_SeoInfosMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };
            existingProduct.SeoInfos = new List<SeoInfo>() { new SeoInfo() { Id = "1", LanguageCode = "en-US", SemanticUrl = "SemanticsUrl3" } };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();
            firstProduct.SeoInfo.SemanticUrl = "SemanticsUrl1";
            secondProduct.SeoInfo.SemanticUrl = "SemanticsUrl2";

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<SeoInfo>[] inspectors = {
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl1"),
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl2"),
                x => Assert.True(x.LanguageCode == "en-US" && x.SemanticUrl == "SemanticsUrl3")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().SeoInfos, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductsTwoProductsSamePropertyName_PropertyValuesMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();
            existingProduct.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "1", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_1_MultivalueDictionary", Value = "2", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "1", ValueType = PropertyValueType.ShortText }
            };
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var firstProduct = GetCsvProductBase();
            var secondProduct = GetCsvProductBase();

            firstProduct.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "CatalogProductProperty_2_MultivalueDictionary", Value = "3", ValueType = PropertyValueType.ShortText },
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_MultivalueDictionary", Value = "1,2", ValueType = PropertyValueType.ShortText }
            };

            secondProduct.PropertyValues = new List<PropertyValue>
            {
                new PropertyValue { PropertyName = "TestCategory_ProductProperty_MultivalueDictionary", Value = "3", ValueType = PropertyValueType.ShortText },
            };

            var list = new List<CsvProduct> { firstProduct, secondProduct };

            var progressInfo = new ExportImportProgressInfo();

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, progressInfo, info => { });

            //Assert
            Action<PropertyValue>[] inspectors = {
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_1" && x.Alias == "1"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_1_MultivalueDictionary_2" && x.Alias == "2"),
                x => Assert.True(x.ValueId == "CatalogProductProperty_2_MultivalueDictionary_3" && x.Alias == "3"),
                x => Assert.True(x.ValueId == "TestCategory_ProductProperty_MultivalueDictionary_1" && x.Alias == "1"),
                x => Assert.True(x.ValueId == "TestCategory_ProductProperty_MultivalueDictionary_3" && x.Alias == "3"),
                x => Assert.True(x.ValueId == "TestCategory_ProductProperty_MultivalueDictionary_2" && x.Alias == "2")
            };
            Assert.Collection(_savedProducts.FirstOrDefault().PropertyValues, inspectors);
            Assert.True(!progressInfo.Errors.Any());
        }

        [Fact]
        public void DoImport_UpdateProductHasPriceCurrency_PriceUpdated()
        {
            //Arrange
            var listPrice = 555.5m;
            var existingPriceId = "ExistingPrice_ID";

            var existingProduct = GetCsvProductBase();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var firstProduct = GetCsvProductBase();
            firstProduct.Prices = new List<Price> { new CsvPrice()
            {
                List = listPrice,
                Sale = listPrice,
                Currency = "EUR",
                MinQuantity = 1
            }};

            _pricesInternal = new List<Price>()
            {
                new Price
                {
                    Currency = "EUR",
                    PricelistId = "DefaultEUR",
                    List = 333.3m,
                    Id = existingPriceId,
                    ProductId = firstProduct.Id,
                    MinQuantity = 2
                }
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { firstProduct }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<Price>[] inspectors =
            {
                x => Assert.True(x.List == listPrice && x.Id == existingPriceId && x.ProductId == firstProduct.Id && x.MinQuantity == 1)
            };
            Assert.Collection(_pricesInternal, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductHasPriceId_PriceUpdated()
        {
            //Arrange
            var listPrice = 555.5m;
            var existingPriceId = "ExistingPrice_ID";
            var existingPriceId2 = "ExistingPrice_ID_2";

            var existingProduct = GetCsvProductBase();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var firstProduct = GetCsvProductBase();
            firstProduct.Prices = new List<Price> {new CsvPrice()
            {
                List = listPrice,
                Sale = listPrice,
                Currency = "EUR",
                Id = existingPriceId
            }};

            _pricesInternal = new List<Price>()
            {
                new Price
                {
                    Currency = "EUR",
                    PricelistId = "DefaultEUR",
                    List = 333.3m,
                    Id = existingPriceId,
                    ProductId = firstProduct.Id
                },
                new Price
                {
                    Currency = "EUR",
                    PricelistId = "DefaultEUR",
                    List = 333.3m,
                    Id = existingPriceId2,
                    ProductId = firstProduct.Id
                }
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { firstProduct }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<Price>[] inspectors =
            {
                x => Assert.True(x.List ==  333.3m && x.Id == existingPriceId2 && x.ProductId == firstProduct.Id),
                x => Assert.True(x.List == listPrice && x.Id == existingPriceId && x.ProductId == firstProduct.Id)
            };
            Assert.Collection(_pricesInternal, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductHasPriceListId_PriceUpdated()
        {
            //Arrange
            var listPrice = 555.5m;
            var existingPriceId = "ExistingPrice_ID";
            var existingPriceId2 = "ExistingPrice_ID_2";

            var existingProduct = GetCsvProductBase();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var firstProduct = GetCsvProductBase();
            firstProduct.Prices = new List<Price> {new CsvPrice()
            {
                List = listPrice,
                Sale = listPrice,
                Currency = "EUR",
                PricelistId = "DefaultEUR",
            }};

            _pricesInternal = new List<Price>()
            {
                new Price
                {
                    Currency = "EUR",
                    PricelistId = "DefaultEUR",
                    List = 333.3m,
                    Id = existingPriceId,
                    ProductId = firstProduct.Id
                },
                new Price
                {
                    Currency = "USD",
                    PricelistId = "DefaultUSD",
                    List = 333.3m,
                    Id = existingPriceId2,
                    ProductId = firstProduct.Id
                }
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { firstProduct }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<Price>[] inspectors =
            {
                x => Assert.True(x.List ==  333.3m && x.PricelistId == "DefaultUSD" && x.Id == existingPriceId2 && x.ProductId == firstProduct.Id),
                x => Assert.True(x.List == listPrice && x.Id == existingPriceId && x.PricelistId == "DefaultEUR" && x.ProductId == firstProduct.Id)
            };
            Assert.Collection(_pricesInternal, inspectors);
        }

        [Fact]
        public void DoImport_UpdateProductsTwoProductDifferentPriceCurrency_PricesMerged()
        {
            //Arrange
            var listPrice = 555.5m;
            var salePrice = 666.6m;
            var existingPriceId = "ExistingPrice_ID";

            var existingProduct = GetCsvProductBase();
            _productsInternal = new List<CatalogProduct> { existingProduct };

            var firstProduct = GetCsvProductBase();
            firstProduct.Prices = new List<Price> { new CsvPrice { List = listPrice, Sale = salePrice, Currency = "EUR" } };

            var secondProduct = GetCsvProductBase();
            secondProduct.Prices = new List<Price> { new CsvPrice { List = listPrice, Sale = salePrice, Currency = "USD" } };

            _pricesInternal = new List<Price>
            {
                new Price {Currency = "EUR",PricelistId = "DefaultEUR",List = 333.3m,Sale = 444.4m,Id = existingPriceId,ProductId = firstProduct.Id},
                new Price {Currency = "USD",PricelistId = "DefaultUSD",List = 444.4m,Sale = 555.5m,Id = existingPriceId,ProductId = firstProduct.Id}
            };

            var target = GetImporter();

            //Act
            target.DoImport(new List<CsvProduct> { firstProduct, secondProduct }, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<Price>[] inspectors =
            {
                x => Assert.True(x.List == listPrice && x.Sale == salePrice && x.Id == existingPriceId && x.ProductId == firstProduct.Id && x.Currency == "EUR"),
                x => Assert.True(x.List == listPrice && x.Sale == salePrice && x.Id == existingPriceId && x.ProductId == firstProduct.Id && x.Currency == "USD")
            };
            Assert.Collection(_pricesInternal, inspectors);
        }


        [Fact]
        public void DoImport_UpdateProducts_OnlyExistringProductsMerged()
        {
            //Arrange
            var existingProduct = GetCsvProductBase();

            _productsInternal = new List<CatalogProduct> { existingProduct };
            var existringCategory = CreateCategory(existingProduct);
            _categoriesInternal.Add(existringCategory);

            var product1 = GetCsvProductBase();
            product1.Id = null;

            var product2 = GetCsvProductBase();
            product2.Id = null;

            var product3 = GetCsvProductBase();
            product3.Code = null;
            product3.Id = null;

            var product4 = GetCsvProductBase();
            product4.Code = null;
            product4.Id = null;

            var list = new List<CsvProduct> { product1, product2, product3, product4 };

            var target = GetImporter();

            //Act
            target.DoImport(list, new CsvImportInfo(), new ExportImportProgressInfo(), info => { });

            //Assert
            Action<CatalogProduct>[] inspectors = {
                x => Assert.True(x.Code == "TST1" && x.Id == "1"),
                x => Assert.True(x.Code != "TST1"),
                x => Assert.True(x.Code != "TST1")
            };
            Assert.Collection(_savedProducts, inspectors);
        }

        [Fact]
        public void DoImport_NewProductWithVariationsProductUseSku()
        {
            //Arrange
            var mainProduct = GetCsvProductBase();
            var variationProduct = GetCsvProductWithMainProduct(mainProduct.Sku);

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { mainProduct, variationProduct }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, exportInfo, info => { });

            //Assert
            Assert.True(variationProduct.MainProductId == mainProduct.Id);
        }

        [Fact]
        public void DoImport_NewProductWithVariationsProductUseId()
        {
            //Arrange
            var mainProduct = GetCsvProductBase();
            var variationProduct = GetCsvProductWithMainProduct(mainProduct.Id);

            var target = GetImporter();

            var exportInfo = new ExportImportProgressInfo();

            //Act
            target.DoImport(new List<CsvProduct> { mainProduct, variationProduct }, new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() }, exportInfo, info => { });

            //Assert
            Assert.True(variationProduct.MainProductId == mainProduct.Id);
        }


        private CsvCatalogImporter GetImporter(IProperyDictionaryItemService propDictItemService = null, IProperyDictionaryItemSearchService propDictItemSearchService = null)
        {

            #region StoreServise

            var storeService = new Mock<IStoreService>();
            storeService.Setup(x => x.SearchStores(It.IsAny<Domain.Store.Model.SearchCriteria>())).Returns(new Domain.Store.Model.SearchResult());

            #endregion

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

            itemService.Setup(x => x.Update(It.IsAny<CatalogProduct[]>())).Callback((CatalogProduct[] products) =>
            {
                _savedProducts = products.ToList();
            });

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
            pricingService.Setup(x => x.SavePrices(It.IsAny<Price[]>())).Callback((Price[] prices) =>
            {
                _pricesInternal.RemoveAll(x => prices.Any(y => y.Id == x.Id));
                foreach (var price in prices)
                {
                    if (price.Id == null)
                        price.Id = Guid.NewGuid().ToString();
                }

                _pricesInternal.AddRange(prices);
            });

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

            var commerceService = new Mock<IFulfillmentCenterSearchService>();
            commerceService.Setup(x => x.SearchCenters(It.IsAny<FulfillmentCenterSearchCriteria>())).Returns(() => new GenericSearchResult<Domain.Inventory.Model.FulfillmentCenter> { Results = _fulfillmentCentersInternal });

            #endregion

            #region PropertyService

            var propertyService = new Mock<IPropertyService>();
            propertyService.Setup(x => x.Update(It.IsAny<Property[]>())).Callback((Property[] properties) => { });

            #endregion

            #region PropertyDictionaryItemService
            if (propDictItemSearchService == null)
            {
                var propDictItemSearchServiceMock = new Mock<IProperyDictionaryItemSearchService>();
                var registeredPropDictionaryItems = new[]
                {
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_MultivalueDictionary_1", PropertyId = "CatalogProductProperty_1_MultivalueDictionary", Alias = "1" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_MultivalueDictionary_2", PropertyId = "CatalogProductProperty_1_MultivalueDictionary", Alias = "2" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_MultivalueDictionary_3", PropertyId = "CatalogProductProperty_1_MultivalueDictionary", Alias = "3" },

                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_MultivalueDictionary_1", PropertyId = "CatalogProductProperty_2_MultivalueDictionary", Alias = "1" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_MultivalueDictionary_2", PropertyId = "CatalogProductProperty_2_MultivalueDictionary", Alias = "2" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_MultivalueDictionary_3", PropertyId = "CatalogProductProperty_2_MultivalueDictionary", Alias = "3" },

                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_Dictionary_1", PropertyId = "CatalogProductProperty_1_Dictionary", Alias = "1" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_Dictionary_2", PropertyId = "CatalogProductProperty_1_Dictionary", Alias = "2" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_1_Dictionary_3", PropertyId = "CatalogProductProperty_1_Dictionary", Alias = "3" },

                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_Dictionary_1", PropertyId = "CatalogProductProperty_2_Dictionary", Alias = "1" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_Dictionary_2", PropertyId = "CatalogProductProperty_2_Dictionary", Alias = "2" },
                    new PropertyDictionaryItem { Id = "CatalogProductProperty_2_Dictionary_3", PropertyId = "CatalogProductProperty_2_Dictionary", Alias = "3" },

                    new PropertyDictionaryItem { Id = "TestCategory_ProductProperty_MultivalueDictionary_1", PropertyId = "TestCategory_ProductProperty_MultivalueDictionary", Alias = "1" },
                    new PropertyDictionaryItem { Id = "TestCategory_ProductProperty_MultivalueDictionary_2", PropertyId = "TestCategory_ProductProperty_MultivalueDictionary", Alias = "2" },
                    new PropertyDictionaryItem { Id = "TestCategory_ProductProperty_MultivalueDictionary_3", PropertyId = "TestCategory_ProductProperty_MultivalueDictionary", Alias = "3" },
                };

                propDictItemSearchServiceMock.Setup(x => x.Search(It.IsAny<PropertyDictionaryItemSearchCriteria>())).Returns(new GenericSearchResult<PropertyDictionaryItem> { Results = registeredPropDictionaryItems.ToList() });
                propDictItemSearchService = propDictItemSearchServiceMock.Object;
            }
            if (propDictItemService == null)
            {
                propDictItemService = new Mock<IProperyDictionaryItemService>().Object;
            }
            #endregion

            #region PricingSearchService

            var pricingSearchService = new Mock<IPricingSearchService>();
            pricingSearchService.Setup(x => x.SearchPrices(It.IsAny<PricesSearchCriteria>()))
                .Returns((PricesSearchCriteria crietera) =>
                {
                    return new PricingSearchResult<Price>
                    {
                        Results = _pricesInternal.Where(x => crietera.ProductIds.Contains(x.ProductId)).Select(TestUtils.Clone).ToList()
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
                settingsManager.Object,
                storeService.Object,
                propDictItemSearchService,
                propDictItemService
            );

            return target;
        }

        private static List<Property> CreateProductPropertiesInCategory(Category category, Catalog catalog)
        {
            var multivalueDictionaryProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_MultivalueDictionary",
                Id = $"{category.Name}_ProductProperty_MultivalueDictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                IsInherited = false,
                ValueType = PropertyValueType.ShortText
            };

            var multivalueProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_Multivalue",
                Id = $"{category.Name}_ProductProperty_Multivalue",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = false,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var dictionaryProperty = new Property
            {
                Name = $"{category.Name}_ProductProperty_Dictionary",
                Id = $"{category.Name}_ProductProperty_Dictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                //Category = category,
                CategoryId = category.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var property = new Property
            {
                Name = $"{category.Name}_ProductProperty",
                Id = $"{category.Name}_ProductProperty",
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

            var catalogProductProperty = new Property
            {
                Name = "CatalogProductProperty_1_MultivalueDictionary",
                Id = "CatalogProductProperty_1_MultivalueDictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var productProperty2Id = Guid.NewGuid().ToString();
            var catalogProductProperty2 = new Property
            {
                Name = "CatalogProductProperty_2_MultivalueDictionary",
                Id = "CatalogProductProperty_2_MultivalueDictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = true,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var catalogProductProperty3 = new Property
            {
                Name = "CatalogProductProperty_1_Multivalue",
                Id = "CatalogProductProperty_1_Multivalue",
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
                Id = "CatalogProductProperty_2_Multivalue",
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
                Id = "CatalogProductProperty_1_Dictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var productProperty6Id = Guid.NewGuid().ToString();
            var catalogProductProperty6 = new Property
            {
                Name = "CatalogProductProperty_2_Dictionary",
                Id = "CatalogProductProperty_2_Dictionary",
                //Catalog = catalog,
                CatalogId = catalog.Id,
                Dictionary = true,
                Multivalue = false,
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };

            var catalogProductProperty7 = new Property
            {
                Name = "CatalogProductProperty_1",
                Id = "CatalogProductProperty_1",
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
                Id = "CatalogProductProperty_2",
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

        private static CsvProduct GetCsvProductWithMainProduct(string mainProductIdOrSku)
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
                Code = "TST2",
                Currency = "USD",
                EditorialReview = review,
                Reviews = new List<EditorialReview> { review },
                Id = "2",
                ListPrice = "100",
                Inventory = new InventoryInfo(),
                SeoInfo = seoInfo,
                SeoInfos = new List<SeoInfo> { seoInfo },
                Name = "TST2-TestCategory",
                Price = new Price(),
                Quantity = "0",
                Sku = "TST2",
                TrackInventory = true,
                MainProductId = mainProductIdOrSku
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