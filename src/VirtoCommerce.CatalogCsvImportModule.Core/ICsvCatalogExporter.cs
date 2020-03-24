using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Core
{
    public interface ICsvCatalogExporter
    {
        void DoExport(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}