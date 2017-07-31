//Call this to register our module to main application
var moduleTemplateName = "virtoCommerce.catalogCsvImportModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleTemplateName);
}

angular.module(moduleTemplateName, [])
.config(['$stateProvider', '$urlRouterProvider',
    function ($stateProvider, $urlRouterProvider) {
    }
])
.run(['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', 'platformWebApp.toolbarService', 'platformWebApp.pushNotificationTemplateResolver', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogCsvImportModule.catalogImportService', 'virtoCommerce.catalogCsvImportModule.catalogExportService', '$state',
    function ($rootScope, mainMenuService, widgetService, toolbarService, pushNotificationTemplateResolver, bladeNavigationService, catalogImportService, catalogExportService, $state) {

        //NOTIFICATIONS
        //Export
        var menuExportTemplate =
        {
            priority: 900,
            satisfy: function (notify, place) { return place == 'menu' && notify.notifyType == 'CatalogCsvExport'; },
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/export/notifications/menuExport.tpl.html',
            action: function (notify) { $state.go('workspace.pushNotificationsHistory', notify) }
        };
        pushNotificationTemplateResolver.register(menuExportTemplate);

        var historyExportTemplate =
        {
            priority: 900,
            satisfy: function (notify, place) { return place == 'history' && notify.notifyType == 'CatalogCsvExport'; },
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/export/notifications/historyExport.tpl.html',
            action: function (notify) {
                var blade = {
                    id: 'CatalogCsvExportDetail',
                    title: 'catalog export detail',
                    subtitle: 'detail',
                    template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/export/catalog-CSV-export.tpl.html',
                    controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVexportController',
                    notification: notify
                };
                bladeNavigationService.showBlade(blade);
            }
        };
        pushNotificationTemplateResolver.register(historyExportTemplate);

        //Import
        var menuImportTemplate =
        {
            priority: 900,
            satisfy: function (notify, place) { return place == 'menu' && notify.notifyType == 'CatalogCsvImport'; },
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/notifications/menuImport.tpl.html',
            action: function (notify) { $state.go('workspace.pushNotificationsHistory', notify) }
        };
        pushNotificationTemplateResolver.register(menuImportTemplate);

        var historyImportTemplate =
        {
            priority: 900,
            satisfy: function (notify, place) { return place == 'history' && notify.notifyType == 'CatalogCsvImport'; },
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/notifications/historyImport.tpl.html',
            action: function (notify) {
                var blade = {
                    id: 'CatalogCsvImportDetail',
                    title: 'catalog import detail',
                    subtitle: 'detail',
                    template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/catalog-CSV-import.tpl.html',
                    controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVimportController',
                    notification: notify
                };
                bladeNavigationService.showBlade(blade);
            }
        };
        pushNotificationTemplateResolver.register(historyImportTemplate);

        // IMPORT / EXPORT
        catalogImportService.register({
            name: 'VirtoCommerce CSV import',
            description: 'Native VirtoCommerce catalog data import from CSV',
            icon: 'fa fa-file-archive-o',
            controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVimportWizardController',
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/wizard/catalog-CSV-import-wizard.tpl.html'
        });
        catalogExportService.register({
            name: 'VirtoCommerce CSV export',
            description: 'Native VirtoCommerce catalog data export to CSV',
            icon: 'fa fa-file-archive-o',
            controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVexportController',
            template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/export/catalog-CSV-export.tpl.html'
        });

        var catalogImportCommand = {
            name: "import",
            icon: 'fa fa-download',
            executeMethod: function (blade) {
                var newBlade = {
                    id: 'catalogImport',
                    title: 'catalog.blades.importers-list.title',
                    subtitle: 'catalog.blades.importers-list.subtitle',
                    catalog: blade.catalog,
                    controller: 'virtoCommerce.catalogCsvImportModule.importerListController',
                    template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/importers-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            },
            canExecuteMethod: function(blade) {
                return blade.catalogId;
            },
            index: 100
        };
        toolbarService.register(catalogImportCommand, 'virtoCommerce.catalogModule.categoriesItemsListController');

        var catalogExportCommand = {
            name: "export",
            icon: 'fa fa-upload',
            executeMethod: function (blade) {

            },
            canExecuteMethod: function(blade) {
                return blade.catalogId;
            },
            index: 101
        };
        toolbarService.register(catalogExportCommand, 'virtoCommerce.catalogModule.categoriesItemsListController');
    }
]);
