using System;
using System.IO;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Core
{
    public interface ICsvCatalogImporter
    {
        void DoImport(Stream inputStream, CsvImportInfo importInfo, Action<ExportImportProgressInfo> progressCallback);
    }
}