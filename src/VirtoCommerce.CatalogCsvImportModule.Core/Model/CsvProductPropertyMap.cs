using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Model
{
    public class CsvProductPropertyMap : ValueObject
    {
        public string EntityColumnName { get; set; }
        public string CsvColumnName { get; set; }
        public string CustomValue { get; set; }

        public override string ToString()
        {
            return $"{(CsvColumnName ?? CustomValue) ?? "none"} -> {EntityColumnName ?? "none"}";
        }
    }
}
