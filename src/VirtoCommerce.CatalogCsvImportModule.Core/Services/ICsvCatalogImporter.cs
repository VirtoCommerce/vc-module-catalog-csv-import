using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Services
{
    public interface ICsvCatalogImporter
    {
        void DoImport(Stream inputStream, CsvImportInfo importInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}
