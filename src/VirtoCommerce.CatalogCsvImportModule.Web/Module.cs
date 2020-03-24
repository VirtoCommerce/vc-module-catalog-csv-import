using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogCsvImportModule.Data.Core;
using VirtoCommerce.CatalogCsvImportModule.Data.Services;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.CatalogCsvImportModule.Web
{
    public class Module : ModuleBase
    {
        // private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetupDatabase()
        {

        }

        public override void Initialize()
        {
            _container.RegisterType<ICsvCatalogExporter, CsvCatalogExporter>();
            _container.RegisterType<ICsvCatalogImporter, CsvCatalogImporter>();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
        }
    }
}
