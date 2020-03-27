using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CsvHelper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogCsvImportModule.Core.Services;
using VirtoCommerce.CatalogCsvImportModule.Web.Model.PushNotifications;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using CsvModuleConstants = VirtoCommerce.CatalogCsvImportModule.Core.ModuleConstants;
using ModuleConstants = VirtoCommerce.CatalogModule.Core.ModuleConstants;

namespace VirtoCommerce.CatalogCsvImportModule.Web.Controllers.Api
{
    [Route("api/catalogcsvimport")]
    public class ExportImportController : Controller
    {
        private readonly ICsvCatalogExporter _csvExporter;
        private readonly ICsvCatalogImporter _csvImporter;

        private readonly ICatalogService _catalogService;
        private readonly IPushNotificationManager _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IUserNameResolver _userNameResolver;
        private readonly ISettingsManager _settingsManager;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public ExportImportController(ICatalogService catalogService,
            IPushNotificationManager pushNotificationManager,
            IAuthorizationService authorizationService,
            ICurrencyService currencyService,
            IBlobStorageProvider blobStorageProvider,
            IBlobUrlResolver blobUrlResolver,
            ICsvCatalogExporter csvExporter,
            ICsvCatalogImporter csvImporter,
            IUserNameResolver userNameResolver,
            ISettingsManager settingsManager)
        {
            _catalogService = catalogService;
            _notifier = pushNotificationManager;
            _authorizationService = authorizationService;
            _currencyService = currencyService;
            _blobStorageProvider = blobStorageProvider;
            _userNameResolver = userNameResolver;
            _settingsManager = settingsManager;
            _blobUrlResolver = blobUrlResolver;

            _csvExporter = csvExporter;
            _csvImporter = csvImporter;
        }

        /// <summary>
        /// Start catalog data export process.
        /// </summary>
        /// <remarks>Data export is an async process. An ExportNotification is returned for progress reporting.</remarks>
        /// <param name="exportInfo">The export configuration.</param>
        [HttpPost]
        [Route("export")]
        [Authorize(ModuleConstants.Security.Permissions.Export)]
        public async Task<ActionResult<ExportNotification>> DoExport([FromBody] CsvExportInfo exportInfo)
        {
            var criteria = AbstractTypeFactory<CatalogSearchCriteria>.TryCreateInstance();
            var ids = new List<string>();
            ids.AddRange(exportInfo.CategoryIds);
            ids.AddRange(exportInfo.ProductIds);
            criteria.ObjectIds = ids.ToArray();


            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }


            var notification = new ExportNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Catalog export task",
                Description = "starting export...."
            };
            await _notifier.SendAsync(notification);


            BackgroundJob.Enqueue(() => BackgroundExport(exportInfo, notification));

            return Ok(notification);
        }

        /// <summary>
        /// Gets the CSV mapping configuration.
        /// </summary>
        /// <remarks>Analyses the supplied file's structure and returns automatic column mapping.</remarks>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("import/mappingconfiguration")]
        public async Task<ActionResult<CsvProductMappingConfiguration>> GetMappingConfiguration([FromQuery] string fileUrl,
            [FromQuery] string delimiter = ";")
        {
            var result = CsvProductMappingConfiguration.GetDefaultConfiguration();
            var decodedDelimiter = HttpUtility.UrlDecode(delimiter);
            result.Delimiter = decodedDelimiter;

            //Read csv headers and try to auto map fields by name

            using (var reader = new CsvReader(new StreamReader(_blobStorageProvider.OpenRead(fileUrl))))
            {
                reader.Configuration.Delimiter = decodedDelimiter;

                if (await reader.ReadAsync())
                {
                    if (reader.ReadHeader())
                    {
                        result.AutoMap(reader.Context.HeaderRecord);
                    }
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Start catalog data import process.
        /// </summary>
        /// <remarks>Data import is an async process. An ImportNotification is returned for progress reporting.</remarks>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize(ModuleConstants.Security.Permissions.Import)]
        public async Task<ActionResult<ImportNotification>> DoImport(CsvImportInfo importInfo)
        {
            var criteria = AbstractTypeFactory<CatalogSearchCriteria>.TryCreateInstance();
            criteria.CatalogIds = new[] { importInfo.CatalogId };

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }


            var notification = new ImportNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Import catalog from CSV",
                Description = "starting import...."
            };
            await _notifier.SendAsync(notification);

            BackgroundJob.Enqueue(() => BackgroundImport(importInfo, notification));

            return Ok(notification);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        // Only public methods can be invoked in the background. (Hangfire)
        public async Task BackgroundImport(CsvImportInfo importInfo, ImportNotification notifyEvent)
        {
            Action<ExportImportProgressInfo> progressCallback = async x =>
            {
                notifyEvent.InjectFrom(x);
                await _notifier.SendAsync(notifyEvent);
            };

            using (var stream = _blobStorageProvider.OpenRead(importInfo.FileUrl))
            {
                try
                {
                    await _csvImporter.DoImportAsync(stream, importInfo, progressCallback);
                }
                catch (Exception ex)
                {
                    notifyEvent.Description = "Export error";
                    notifyEvent.ErrorCount++;
                    notifyEvent.Errors.Add(ex.ToString());
                }
                finally
                {
                    notifyEvent.Finished = DateTime.UtcNow;
                    notifyEvent.Description = "Import finished" + (notifyEvent.Errors.Any() ? " with errors" : " successfully");
                    await _notifier.SendAsync(notifyEvent);
                }
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        // Only public methods can be invoked in the background. (Hangfire)
        public async Task BackgroundExport(CsvExportInfo exportInfo, ExportNotification notifyEvent)
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();
            var defaultCurrency = currencies.First(x => x.IsPrimary);
            exportInfo.Currency ??= defaultCurrency.Code;
            var catalog = await _catalogService.GetByIdsAsync(new[] { exportInfo.CatalogId });
            if (catalog == null)
            {
                throw new NullReferenceException("catalog");
            }

            Action<ExportImportProgressInfo> progressCallback = async x =>
            {
                notifyEvent.InjectFrom(x);
                await _notifier.SendAsync(notifyEvent);
            };

            using (var stream = new MemoryStream())
            {
                try
                {
                    exportInfo.Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration();
                    await _csvExporter.DoExportAsync(stream, exportInfo, progressCallback);

                    stream.Position = 0;
                    var fileNameTemplate = await _settingsManager.GetValueAsync(CsvModuleConstants.Settings.General.ExportFileNameTemplate.Name, CsvModuleConstants.Settings.General.ExportFileNameTemplate.DefaultValue.ToString());
                    var fileName = string.Format(fileNameTemplate, DateTime.UtcNow);
                    fileName = Path.ChangeExtension(fileName, ".csv");

                    var blobRelativeUrl = Path.Combine("temp", fileName);

                    //Upload result csv to blob storage
                    using (var blobStream = _blobStorageProvider.OpenWrite(blobRelativeUrl))
                    {
                        stream.CopyTo(blobStream);
                    }

                    //Get a download url
                    notifyEvent.DownloadUrl = _blobUrlResolver.GetAbsoluteUrl(blobRelativeUrl);
                    notifyEvent.Description = "Export finished";
                }
                catch (Exception ex)
                {
                    notifyEvent.Description = "Export failed";
                    notifyEvent.Errors.Add(ex.ExpandExceptionMessage());
                }
                finally
                {
                    notifyEvent.Finished = DateTime.UtcNow;
                    await _notifier.SendAsync(notifyEvent);
                }
            }
        }
    }
}
