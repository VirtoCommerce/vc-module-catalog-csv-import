using VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Model
{
    public class CsvPrice : Price
    {
        public virtual void MergeFrom(Price source)
        {
            Id = source.Id;
            Sale = Sale ?? source.Sale;
            List = List == 0M ? source.List : List;
            MinQuantity = source.MinQuantity;
            PricelistId = source.PricelistId;
        }
    }
}
