using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Services
{
    public interface ICsvCatalogExporter
    {
        void DoExport(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}
