namespace VirtoCommerce.CatalogCsvImportModule.Data.Model
{
    public class CsvImportInfo
    {
        public string CatalogId { get; set; }
        public string FileUrl { get; set; }
        public CsvProductMappingConfiguration Configuration { get; set; }
        public CsvSetting CsvSettings { get; set; }
    }
}