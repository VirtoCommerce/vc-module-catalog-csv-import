using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Model
{
    public sealed class CsvProduct : CatalogProduct
    {
        private readonly string[] _csvCellDelimiter = { "--", "|" };
        private readonly IBlobUrlResolver _blobUrlResolver;
        public CsvProduct()
        {
            SeoInfos = new List<SeoInfo>();
            Reviews = new List<EditorialReview>();
            Properties = new List<Property>();
            Images = new List<Image>();
            Assets = new List<Asset>();
            Price = new CsvPrice { Currency = "USD" };
            Prices = new List<Price> { Price };
            Inventory = new InventoryInfo();
            EditorialReview = new EditorialReview();
            Reviews = new List<EditorialReview> { EditorialReview };
            SeoInfo = new CsvSeoInfo { ObjectType = typeof(CatalogProduct).Name };
            SeoInfos = new List<SeoInfo> { SeoInfo };
        }

        public CsvProduct(CatalogProduct product, IBlobUrlResolver blobUrlResolver, Price price, InventoryInfo inventory, SeoInfo seoInfo)
            : this()
        {
            _blobUrlResolver = blobUrlResolver;

            this.InjectFrom(product);
            Properties = product.Properties;
            Images = product.Images;
            Assets = product.Assets;
            Links = product.Links;
            Variations = product.Variations;
            SeoInfos = product.SeoInfos;
            Reviews = product.Reviews;
            Associations = product.Associations;
            if (price != null)
            {
                Price = price;
            }
            if (inventory != null)
            {
                Inventory = inventory;
            }
            if (seoInfo != null)
            {
                SeoInfo = seoInfo;
            }
        }
        public Price Price { get; set; }
        public InventoryInfo Inventory { get; set; }
        public EditorialReview EditorialReview { get; set; }
        public SeoInfo SeoInfo { get; set; }
        public IList<Price> Prices { get; set; }
        public string PriceId
        {
            get
            {
                return Price.Id;
            }
            set
            {
                Price.Id = value;
            }
        }
        public string SalePrice
        {
            get
            {
                return Price.Sale?.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Price.Sale = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }
            }
        }

        public string ListPrice
        {
            get
            {
                return Price.List.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                Price.List = string.IsNullOrEmpty(value) ? 0 : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
        }

        public string PriceMinQuantity
        {
            get
            {
                return Price.MinQuantity.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                Price.MinQuantity = Convert.ToInt32(value);
            }
        }

        public string Currency
        {
            get
            {
                return Price.Currency;
            }
            set
            {
                Price.Currency = value;
            }
        }

        public string PriceListId
        {
            get
            {
                return Price.PricelistId;
            }
            set
            {
                Price.PricelistId = value;
            }
        }

        public string FulfillmentCenterId
        {
            get
            {
                return Inventory.FulfillmentCenterId;
            }
            set
            {
                Inventory.FulfillmentCenterId = value;
            }
        }

        public string Quantity
        {
            get
            {
                return Inventory.InStockQuantity.ToString();
            }
            set
            {
                Inventory.InStockQuantity = Convert.ToInt64(value);
            }
        }

        public string PrimaryImage
        {
            get
            {
                var retVal = string.Empty;
                if (Images != null)
                {
                    var primaryImage = Images.OrderBy(x => x.SortOrder).FirstOrDefault();
                    if (primaryImage != null)
                    {
                        retVal = _blobUrlResolver != null ? _blobUrlResolver.GetAbsoluteUrl(primaryImage.Url) : primaryImage.Url;
                    }
                }
                return retVal;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Images.Add(new Image
                    {
                        Url = value,
                        SortOrder = 0,
                        Group = "images",
                        Name = value.Split('/').Last()
                    });
                }
            }
        }

        public string AltImage
        {
            get
            {
                var retVal = string.Empty;
                if (Images != null)
                {
                    var primaryImage = Images.OrderBy(x => x.SortOrder).Skip(1).FirstOrDefault();
                    if (primaryImage != null)
                    {
                        retVal = _blobUrlResolver != null ? _blobUrlResolver.GetAbsoluteUrl(primaryImage.Url) : primaryImage.Url;
                    }
                }
                return retVal;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var altImages = value.Split(_csvCellDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string url in altImages)
                    {
                        Images.Add(new Image
                        {
                            Url = url,
                            SortOrder = 1,
                            Group = "images",
                            Name = url.Split('/').Last()
                        });
                    }
                }
            }
        }
        public string Sku
        {
            get
            {
                return Code;
            }
            set
            {
                Code = value?.Trim();
            }
        }

        public string ParentSku { get; set; }

        public string CategoryPath
        {
            get
            {
                if (Category == null)
                    return null;

                return Category.Path;
            }
            set
            {
                Category = new Category { Path = value };
            }
        }

        public string ReviewType
        {
            get { return EditorialReview.ReviewType; }
            set { EditorialReview.ReviewType = value; }
        }

        public string Review
        {
            get { return EditorialReview.Content; }
            set { EditorialReview.Content = value; }
        }

        public string SeoTitle
        {
            get { return SeoInfo.PageTitle; }
            set { SeoInfo.PageTitle = value; }
        }

        public string SeoUrl
        {
            get { return SeoInfo.SemanticUrl; }
            set
            {
                var slug = value;
                SeoInfo.SemanticUrl = slug.Substring(0, Math.Min(slug.Length, 240));
            }
        }

        public string SeoDescription
        {
            get { return SeoInfo.MetaDescription; }
            set { SeoInfo.MetaDescription = value; }
        }

        public string SeoLanguage
        {
            get { return SeoInfo.LanguageCode; }
            set { SeoInfo.LanguageCode = value; }
        }

        public string SeoStore
        {
            get { return SeoInfo.StoreId; }
            set { SeoInfo.StoreId = value; }
        }

        public int LineNumber { get; set; }

        /// <summary>
        /// Merge from other product, without any deletion, only update and create allowed
        ///
        /// </summary>
        /// <param name="product"></param>
        public void MergeFrom(CatalogProduct product)
        {
            Id = product.Id;

            if (string.IsNullOrEmpty(Code))
            {
                Code = product.Code;
            }

            if (string.IsNullOrEmpty(Name))
            {
                Name = product.Name;
            }

            if (string.IsNullOrEmpty(CategoryId))
            {
                CategoryId = product.CategoryId;
            }

            if (Category == null || (Category != null && string.IsNullOrEmpty(Category.Path)))
            {
                Category = product.Category;
            }

            if (string.IsNullOrEmpty(ProductType))
            {
                ProductType = product.ProductType;
            }

            if (string.IsNullOrEmpty(Vendor))
            {
                Vendor = product.Vendor;
            }

            var imgComparer = AnonymousComparer.Create((Image x) => x.Url);
            Images = Images.Concat(product.Images).Distinct(imgComparer).ToList();

            var assetComparer = AnonymousComparer.Create((Asset x) => x.Url);
            Assets = Assets.Concat(product.Assets).Distinct(assetComparer).ToList();

            var reviewsComparer = AnonymousComparer.Create((EditorialReview x) => string.Join(":", x.ReviewType, x.LanguageCode));
            Reviews = Reviews.Concat(product.Reviews).Distinct(reviewsComparer).ToList();

            // Merge Properties - leave properties that are not presented in CSV and add all from the CSV (with merging metadata and replacing existing ones)
            var propertyComparer = AnonymousComparer.Create((Property x) => x.Name);
            var skippedExistingProperties = new List<Property>();

            foreach (var property in Properties.OfType<CsvProperty>())
            {
                var existingProperty = product.Properties.FirstOrDefault(x => propertyComparer.Equals(x, property));
                if (existingProperty != null)
                {
                    property.MergeFrom(existingProperty);
                    skippedExistingProperties.Add(existingProperty);
                }
            }

            Properties = Properties.Where(x => !x.Name.IsNullOrEmpty())
                .Concat(product.Properties
                    .Where(x => !skippedExistingProperties.Any(existingProperty => propertyComparer.Equals(x, existingProperty))))
                .ToList();

            //merge seo infos
            var seoComparer = AnonymousComparer.Create((SeoInfo x) => string.Join(":", x.SemanticUrl, x.LanguageCode?.ToLower(), x.StoreId));

            foreach (var seoInfo in SeoInfos.OfType<CsvSeoInfo>())
            {
                var existingSeoInfo = product.SeoInfos.FirstOrDefault(x => seoComparer.Equals(x, seoInfo));
                if (existingSeoInfo != null)
                {
                    seoInfo.MergeFrom(existingSeoInfo);
                    product.SeoInfos.Remove(existingSeoInfo);
                }
            }
            SeoInfos = SeoInfos.Where(x => !x.SemanticUrl.IsNullOrEmpty()).Concat(product.SeoInfos).ToList();
        }
    }
}
