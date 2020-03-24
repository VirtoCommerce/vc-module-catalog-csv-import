using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogCsvImportModule.Core
{
    public interface ICsvCatalogExporter
    {
        void DoExport(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}
