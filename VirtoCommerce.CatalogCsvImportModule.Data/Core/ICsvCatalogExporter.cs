using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Core
{
    public interface ICsvCatalogExporter
    {
        void DoExport(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}