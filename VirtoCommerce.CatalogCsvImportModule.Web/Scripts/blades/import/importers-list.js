angular.module('virtoCommerce.catalogCsvImportModule')
    .controller('virtoCommerce.catalogCsvImportModule.importerListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogCsvImportModule.catalogImportService', 'platformWebApp.settings',
    function ($scope, bladeNavigationService, catalogImportService, settings) {

        var blade = $scope.blade;
        var csvSettings = {};

        $scope.selectedNodeId = null;

        function initializeBlade() {

            settings.getSettings({
                id: 'VirtoCommerce.CatalogCsvImportModule'
            }, function (csvImportSettingsData) {
                var tempMiltiValues =
                    _.findWhere(csvImportSettingsData, { name: 'CsvCatalogImport.UpdateMultiValues' }).value;
                csvSettings.overWriteMultiValue = tempMiltiValues.toLowerCase() === 'true';
            });

            $scope.registrationsList = catalogImportService.registrationsList;
            blade.isLoading = false;
        };

        $scope.openBlade = function (data) {
            var newBlade = {};
            angular.copy(data, newBlade);
            newBlade.catalog = blade.catalog;
            newBlade.csvSettings = csvSettings;
            bladeNavigationService.showBlade(newBlade, blade.parentBlade);
        }

        $scope.bladeHeadIco = 'fa fa-upload';

        initializeBlade();
    }]);