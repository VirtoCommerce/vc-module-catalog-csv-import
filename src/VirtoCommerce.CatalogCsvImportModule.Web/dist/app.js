/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 0);
/******/ })
/************************************************************************/
/******/ ({

/***/ "./Scripts/blades/export/catalog-CSV-export.js":
/*!*****************************************************!*\
  !*** ./Scripts/blades/export/catalog-CSV-export.js ***!
  \*****************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

angular.module('virtoCommerce.catalogCsvImportModule')
.controller('virtoCommerce.catalogCsvImportModule.catalogCSVexportController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogCsvImportModule.export', 'virtoCommerce.inventoryModule.fulfillments', 'virtoCommerce.pricingModule.pricelists',
    function ($scope, bladeNavigationService, exportResourse, fulfillments, pricelists) {

        const blade = $scope.blade;
        blade.fulfilmentCenterId = undefined;
        blade.pricelistId = undefined;
        blade.isLoading = false;
        blade.title = 'catalogCsvImportModule.blades.catalog-CSV-export.title';
        blade.titleValues = { name: blade.catalog ? blade.catalog.name : '' };

        function initializeBlade() {
            fulfillments.search({}, function (data) {
                    if(data.totalCount > 0){
                        $scope.fulfillmentCenters = data.results;
                        blade.fulfilmentCenterId = _.first(data.results).id;
                    }
                },
                function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
        }

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });

        $scope.startExport = function () {
            exportResourse.run({
                catalogId: blade.catalog.id,
                categoryIds: _.map(blade.selectedCategories, function (x) { return x.id }),
                productIds: _.map(blade.selectedProducts, function (x) { return x.id }),
                fulfilmentCenterId: blade.fulfilmentCenterId,
                pricelistId: blade.pricelistId
            },
            function (data) { blade.notification = data; },
            function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
        };

        $scope.setForm = function (form) {
            $scope.formScope = form;
        };

        pricelists.search({ take: 1000 }, function (result) {
            $scope.pricelists = _.filter(result.results,
                function (x) {
                    return _.some(x.assignments,
                        function (y) {
                            return y.catalogId === blade.catalog.id
                        })
                });
        });

        $scope.blade.headIcon = 'fa-file-archive-o';

        initializeBlade();
    }]);


/***/ }),

/***/ "./Scripts/blades/import/catalog-CSV-import.js":
/*!*****************************************************!*\
  !*** ./Scripts/blades/import/catalog-CSV-import.js ***!
  \*****************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.controller('virtoCommerce.catalogCsvImportModule.catalogCSVimportController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogCsvImportModule.import', 'platformWebApp.notifications',
    function ($scope, bladeNavigationService, importResourse, notificationsResource) {

        var blade = $scope.blade;
        blade.isLoading = false;
        blade.title = 'catalogCsvImportModule.blades.catalog-CSV-import.title';

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id == blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });

        $scope.setForm = function (form) {
            $scope.formScope = form;
        }

        $scope.bladeHeadIco = 'fa fa-file-archive-o';


    }]);


/***/ }),

/***/ "./Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.js":
/*!********************************************************************************!*\
  !*** ./Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.js ***!
  \********************************************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.controller('virtoCommerce.catalogCsvImportModule.catalogCSVimportWizardMappingStepController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
	var blade = $scope.blade;
	

	//Need automatically tracking 
	angular.forEach($scope.blade.importConfiguration.propertyMaps, function (x) {
		$scope.$watch(function ($scope) { return x.csvColumnName }, function (newValue, oldValue) {
			if (oldValue != newValue) {
				if (newValue) {
					var index = blade.importConfiguration.propertyCsvColumns.indexOf(newValue);
					if (index != -1) {
						blade.importConfiguration.propertyCsvColumns.splice(index, 1);
					}
				}
				if (oldValue) {
					var index = blade.importConfiguration.propertyCsvColumns.indexOf(oldValue);
					if (index == -1) {
						blade.importConfiguration.propertyCsvColumns.push(oldValue);
					}
				}
			}
		});
	});

	$scope.clearPropertyCsvColumns = function () {
		blade.importConfiguration.propertyCsvColumns.length = 0;

	};

	$scope.removePropertyCsvColumn = function (column) {
		var index = blade.importConfiguration.propertyCsvColumns.indexOf(column);
		if (index != -1) {
			blade.importConfiguration.propertyCsvColumns.splice(index, 1);
		}
	};

    $scope.setForm = function (form) {
        $scope.formScope = form;
    }

    $scope.isValid = function () {
    	return $scope.formScope && $scope.formScope.$valid;
    };

    $scope.saveChanges = function () {
    	$scope.bladeClose();
    };

    blade.isLoading = false;
}]);




/***/ }),

/***/ "./Scripts/blades/import/wizard/catalog-CSV-import-wizard.js":
/*!*******************************************************************!*\
  !*** ./Scripts/blades/import/wizard/catalog-CSV-import-wizard.js ***!
  \*******************************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.controller('virtoCommerce.catalogCsvImportModule.catalogCSVimportWizardController', ['$scope', '$localStorage', 'platformWebApp.bladeNavigationService', 'FileUploader', 'virtoCommerce.catalogCsvImportModule.import',
    function ($scope, $localStorage, bladeNavigationService, FileUploader, importResource) {

        var blade = $scope.blade;
        blade.isLoading = false;
        blade.title = 'catalogCsvImportModule.wizards.catalog-CSV-import.title';
        blade.subtitle = 'catalogCsvImportModule.wizards.catalog-CSV-import.subtitle';
        blade.subtitleVales = { name: blade.catalog.name };

        $scope.columnDelimiters = [
            { name: "catalogCsvImportModule.wizards.catalog-CSV-import.labels.space", value: " " },
            { name: "catalogCsvImportModule.wizards.catalog-CSV-import.labels.comma", value: "," },
            { name: "catalogCsvImportModule.wizards.catalog-CSV-import.labels.semicolon", value: ";" },
            { name: "catalogCsvImportModule.wizards.catalog-CSV-import.labels.tab", value: "\t" }
        ];

        if (!$scope.uploader) {
            // create the uploader
            var uploader = $scope.uploader = new FileUploader({
                scope: $scope,
                headers: { Accept: 'application/json' },
                url: 'api/platform/assets?folderUrl=tmp',
                method: 'POST',
                autoUpload: true,
                removeAfterUpload: true
            });

            // ADDING FILTERS: csv only
            //uploader.filters.push({
            //    name: 'csvFilter',
            //    fn: function (i /*{File|FileLikeObject}*/, options) {
            //        var type = '|' + i.type.slice(i.type.lastIndexOf('/') + 1) + '|';
            //        return '|csv|vnd.ms-excel|'.indexOf(type) !== -1;
            //    }
            //});

            uploader.onBeforeUploadItem = function (fileItem) {
                blade.isLoading = true;
            };

            uploader.onSuccessItem = function (fileItem, asset, status, headers) {
                blade.csvFileUrl = asset[0].relativeUrl;

                importResource.getMappingConfiguration({ fileUrl: blade.csvFileUrl, delimiter: encodeURIComponent(blade.columnDelimiter) }, function (data) {
                    if ($localStorage.lastKnownImportData && $localStorage.lastKnownImportData.eTag === data.eTag) {
                        angular.extend(data, $localStorage.lastKnownImportData);
                    }

                    blade.importConfiguration = data;
                    blade.isLoading = false;
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            };

            uploader.onAfterAddingAll = function (addedItems) {
                bladeNavigationService.setError(null, blade);
            };

            uploader.onErrorItem = function (item, response, status, headers) {
                bladeNavigationService.setError(item._file.name + ' failed: ' + (response.message ? response.message : status), blade);
            };
        };

        $scope.canMapColumns = function () {
            return blade.importConfiguration && $scope.formScope && $scope.formScope.$valid;
        }

        $scope.openMappingStep = function () {
            var newBlade = {
                id: "importMapping",
                importConfiguration: blade.importConfiguration,
                title: 'catalogCsvImportModule.wizards.catalog-CSV-import-wizard-mapping-step.title',
                subtitle: 'catalogCsvImportModule.wizards.catalog-CSV-import-wizard-mapping-step.subtitle',
                controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVimportWizardMappingStepController',
                template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.tpl.html'
            };

            blade.canImport = true;
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.startImport = function () {
            $localStorage.lastKnownImportData = {
                eTag: blade.importConfiguration.eTag,
                propertyMaps: blade.importConfiguration.propertyMaps,
                propertyCsvColumns: blade.importConfiguration.propertyCsvColumns
            };

            var exportInfo = { configuration: blade.importConfiguration, fileUrl: blade.csvFileUrl, catalogId: blade.catalog.id };
            importResource.run(exportInfo, function (notification) {
                var newBlade = {
                    id: "importProgress",
                    catalog: blade.catalog,
                    notification: notification,
                    importConfiguration: blade.importConfiguration,
                    controller: 'virtoCommerce.catalogCsvImportModule.catalogCSVimportController',
                    template: 'Modules/$(VirtoCommerce.CatalogCsvImportModule)/Scripts/blades/import/catalog-CSV-import.tpl.html'
                };

                $scope.$on("new-notification-event", function (event, notification) {
                    if (notification && notification.id == newBlade.notification.id) {
                        blade.canImport = notification.finished != null;
                    }
                });

                blade.canImport = false;
                bladeNavigationService.showBlade(newBlade, blade);

            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        };

        $scope.setForm = function (form) {
            $scope.formScope = form;
        };

    }]);


/***/ }),

/***/ "./Scripts/resources/VirtoCommerce.CatalogCsvImportModule.WebApi.js":
/*!**************************************************************************!*\
  !*** ./Scripts/resources/VirtoCommerce.CatalogCsvImportModule.WebApi.js ***!
  \**************************************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.factory('virtoCommerce.catalogCsvImportModule.WebApi', ['$resource', function ($resource) {
    return $resource('api/catalogcsvimport');
}]);


/***/ }),

/***/ "./Scripts/resources/export.js":
/*!*************************************!*\
  !*** ./Scripts/resources/export.js ***!
  \*************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.factory('virtoCommerce.catalogCsvImportModule.export', ['$resource', function ($resource) {

    return $resource('api/catalogcsvimport/export/:id', { id: '@id' }, {
        run: { method: 'POST', url: 'api/catalogcsvimport/export', isArray: false }
    });

}]);

/***/ }),

/***/ "./Scripts/resources/import.js":
/*!*************************************!*\
  !*** ./Scripts/resources/import.js ***!
  \*************************************/
/*! no static exports found */
/***/ (function(module, exports) {

﻿angular.module('virtoCommerce.catalogCsvImportModule')
.factory('virtoCommerce.catalogCsvImportModule.import', ['$resource', function ($resource) {

    return $resource('api/catalogcsvimport/import', {}, {
        getMappingConfiguration: { method: 'GET', url: 'api/catalogcsvimport/import/mappingconfiguration', isArray: false },
        run: { method: 'POST', url: 'api/catalogcsvimport/import', isArray: false }
	});

}]);

/***/ }),

/***/ "./Scripts/virtoCommerce.catalogCsvImportModule.js":
/*!*********************************************************!*\
  !*** ./Scripts/virtoCommerce.catalogCsvImportModule.js ***!
  \*********************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

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
    .run(['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', 'platformWebApp.toolbarService', 'platformWebApp.pushNotificationTemplateResolver', 'platformWebApp.bladeNavigationService', '$state', 'virtoCommerce.catalogModule.catalogImportService', 'virtoCommerce.catalogModule.catalogExportService',
        function ($rootScope, mainMenuService, widgetService, toolbarService, pushNotificationTemplateResolver, bladeNavigationService, $state, catalogImportService, catalogExportService) {

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
                    title: 'catalogCsvImportModule.blades.history.export.title',
                    subtitle: 'catalogCsvImportModule.blades.history.export.subtitle',
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
                    title: 'catalogCsvImportModule.blades.history.import.title',
                    subtitle: 'catalogCsvImportModule.blades.history.import.title',
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
    }
]);


/***/ }),

/***/ 0:
/*!*******************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** multi ./Scripts/virtoCommerce.catalogCsvImportModule.js ./Scripts/blades/export/catalog-CSV-export.js ./Scripts/blades/import/catalog-CSV-import.js ./Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.js ./Scripts/blades/import/wizard/catalog-CSV-import-wizard.js ./Scripts/resources/export.js ./Scripts/resources/import.js ./Scripts/resources/VirtoCommerce.CatalogCsvImportModule.WebApi.js ***!
  \*******************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

__webpack_require__(/*! ./Scripts/virtoCommerce.catalogCsvImportModule.js */"./Scripts/virtoCommerce.catalogCsvImportModule.js");
__webpack_require__(/*! ./Scripts/blades/export/catalog-CSV-export.js */"./Scripts/blades/export/catalog-CSV-export.js");
__webpack_require__(/*! ./Scripts/blades/import/catalog-CSV-import.js */"./Scripts/blades/import/catalog-CSV-import.js");
__webpack_require__(/*! ./Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.js */"./Scripts/blades/import/wizard/catalog-CSV-import-wizard-mapping-step.js");
__webpack_require__(/*! ./Scripts/blades/import/wizard/catalog-CSV-import-wizard.js */"./Scripts/blades/import/wizard/catalog-CSV-import-wizard.js");
__webpack_require__(/*! ./Scripts/resources/export.js */"./Scripts/resources/export.js");
__webpack_require__(/*! ./Scripts/resources/import.js */"./Scripts/resources/import.js");
module.exports = __webpack_require__(/*! ./Scripts/resources/VirtoCommerce.CatalogCsvImportModule.WebApi.js */"./Scripts/resources/VirtoCommerce.CatalogCsvImportModule.WebApi.js");


/***/ })

/******/ });
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly9WaXJ0b0NvbW1lcmNlLkNhdGFsb2dDc3ZJbXBvcnQvd2VicGFjay9ib290c3RyYXAiLCJ3ZWJwYWNrOi8vVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0Ly4vU2NyaXB0cy9ibGFkZXMvZXhwb3J0L2NhdGFsb2ctQ1NWLWV4cG9ydC5qcyIsIndlYnBhY2s6Ly9WaXJ0b0NvbW1lcmNlLkNhdGFsb2dDc3ZJbXBvcnQvLi9TY3JpcHRzL2JsYWRlcy9pbXBvcnQvY2F0YWxvZy1DU1YtaW1wb3J0LmpzIiwid2VicGFjazovL1ZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydC8uL1NjcmlwdHMvYmxhZGVzL2ltcG9ydC93aXphcmQvY2F0YWxvZy1DU1YtaW1wb3J0LXdpemFyZC1tYXBwaW5nLXN0ZXAuanMiLCJ3ZWJwYWNrOi8vVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0Ly4vU2NyaXB0cy9ibGFkZXMvaW1wb3J0L3dpemFyZC9jYXRhbG9nLUNTVi1pbXBvcnQtd2l6YXJkLmpzIiwid2VicGFjazovL1ZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydC8uL1NjcmlwdHMvcmVzb3VyY2VzL1ZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5XZWJBcGkuanMiLCJ3ZWJwYWNrOi8vVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0Ly4vU2NyaXB0cy9yZXNvdXJjZXMvZXhwb3J0LmpzIiwid2VicGFjazovL1ZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydC8uL1NjcmlwdHMvcmVzb3VyY2VzL2ltcG9ydC5qcyIsIndlYnBhY2s6Ly9WaXJ0b0NvbW1lcmNlLkNhdGFsb2dDc3ZJbXBvcnQvLi9TY3JpcHRzL3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5qcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO1FBQUE7UUFDQTs7UUFFQTtRQUNBOztRQUVBO1FBQ0E7UUFDQTtRQUNBO1FB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LyBUaGUgbW9kdWxlIGNhY2hlXG4gXHR2YXIgaW5zdGFsbGVkTW9kdWxlcyA9IHt9O1xuXG4gXHQvLyBUaGUgcmVxdWlyZSBmdW5jdGlvblxuIFx0ZnVuY3Rpb24gX193ZWJwYWNrX3JlcXVpcmVfXyhtb2R1bGVJZCkge1xuXG4gXHRcdC8vIENoZWNrIGlmIG1vZHVsZSBpcyBpbiBjYWNoZVxuIFx0XHRpZihpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXSkge1xuIFx0XHRcdHJldHVybiBpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXS5leHBvcnRzO1xuIFx0XHR9XG4gXHRcdC8vIENyZWF0ZSBhIG5ldyBtb2R1bGUgKGFuZCBwdXQgaXQgaW50byB0aGUgY2FjaGUpXG4gXHRcdHZhciBtb2R1bGUgPSBpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXSA9IHtcbiBcdFx0XHRpOiBtb2R1bGVJZCxcbiBcdFx0XHRsOiBmYWxzZSxcbiBcdFx0XHRleHBvcnRzOiB7fVxuIFx0XHR9O1xuXG4gXHRcdC8vIEV4ZWN1dGUgdGhlIG1vZHVsZSBmdW5jdGlvblxuIFx0XHRtb2R1bGVzW21vZHVsZUlkXS5jYWxsKG1vZHVsZS5leHBvcnRzLCBtb2R1bGUsIG1vZHVsZS5leHBvcnRzLCBfX3dlYnBhY2tfcmVxdWlyZV9fKTtcblxuIFx0XHQvLyBGbGFnIHRoZSBtb2R1bGUgYXMgbG9hZGVkXG4gXHRcdG1vZHVsZS5sID0gdHJ1ZTtcblxuIFx0XHQvLyBSZXR1cm4gdGhlIGV4cG9ydHMgb2YgdGhlIG1vZHVsZVxuIFx0XHRyZXR1cm4gbW9kdWxlLmV4cG9ydHM7XG4gXHR9XG5cblxuIFx0Ly8gZXhwb3NlIHRoZSBtb2R1bGVzIG9iamVjdCAoX193ZWJwYWNrX21vZHVsZXNfXylcbiBcdF9fd2VicGFja19yZXF1aXJlX18ubSA9IG1vZHVsZXM7XG5cbiBcdC8vIGV4cG9zZSB0aGUgbW9kdWxlIGNhY2hlXG4gXHRfX3dlYnBhY2tfcmVxdWlyZV9fLmMgPSBpbnN0YWxsZWRNb2R1bGVzO1xuXG4gXHQvLyBkZWZpbmUgZ2V0dGVyIGZ1bmN0aW9uIGZvciBoYXJtb255IGV4cG9ydHNcbiBcdF9fd2VicGFja19yZXF1aXJlX18uZCA9IGZ1bmN0aW9uKGV4cG9ydHMsIG5hbWUsIGdldHRlcikge1xuIFx0XHRpZighX193ZWJwYWNrX3JlcXVpcmVfXy5vKGV4cG9ydHMsIG5hbWUpKSB7XG4gXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIG5hbWUsIHsgZW51bWVyYWJsZTogdHJ1ZSwgZ2V0OiBnZXR0ZXIgfSk7XG4gXHRcdH1cbiBcdH07XG5cbiBcdC8vIGRlZmluZSBfX2VzTW9kdWxlIG9uIGV4cG9ydHNcbiBcdF9fd2VicGFja19yZXF1aXJlX18uciA9IGZ1bmN0aW9uKGV4cG9ydHMpIHtcbiBcdFx0aWYodHlwZW9mIFN5bWJvbCAhPT0gJ3VuZGVmaW5lZCcgJiYgU3ltYm9sLnRvU3RyaW5nVGFnKSB7XG4gXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIFN5bWJvbC50b1N0cmluZ1RhZywgeyB2YWx1ZTogJ01vZHVsZScgfSk7XG4gXHRcdH1cbiBcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsICdfX2VzTW9kdWxlJywgeyB2YWx1ZTogdHJ1ZSB9KTtcbiBcdH07XG5cbiBcdC8vIGNyZWF0ZSBhIGZha2UgbmFtZXNwYWNlIG9iamVjdFxuIFx0Ly8gbW9kZSAmIDE6IHZhbHVlIGlzIGEgbW9kdWxlIGlkLCByZXF1aXJlIGl0XG4gXHQvLyBtb2RlICYgMjogbWVyZ2UgYWxsIHByb3BlcnRpZXMgb2YgdmFsdWUgaW50byB0aGUgbnNcbiBcdC8vIG1vZGUgJiA0OiByZXR1cm4gdmFsdWUgd2hlbiBhbHJlYWR5IG5zIG9iamVjdFxuIFx0Ly8gbW9kZSAmIDh8MTogYmVoYXZlIGxpa2UgcmVxdWlyZVxuIFx0X193ZWJwYWNrX3JlcXVpcmVfXy50ID0gZnVuY3Rpb24odmFsdWUsIG1vZGUpIHtcbiBcdFx0aWYobW9kZSAmIDEpIHZhbHVlID0gX193ZWJwYWNrX3JlcXVpcmVfXyh2YWx1ZSk7XG4gXHRcdGlmKG1vZGUgJiA4KSByZXR1cm4gdmFsdWU7XG4gXHRcdGlmKChtb2RlICYgNCkgJiYgdHlwZW9mIHZhbHVlID09PSAnb2JqZWN0JyAmJiB2YWx1ZSAmJiB2YWx1ZS5fX2VzTW9kdWxlKSByZXR1cm4gdmFsdWU7XG4gXHRcdHZhciBucyA9IE9iamVjdC5jcmVhdGUobnVsbCk7XG4gXHRcdF9fd2VicGFja19yZXF1aXJlX18ucihucyk7XG4gXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShucywgJ2RlZmF1bHQnLCB7IGVudW1lcmFibGU6IHRydWUsIHZhbHVlOiB2YWx1ZSB9KTtcbiBcdFx0aWYobW9kZSAmIDIgJiYgdHlwZW9mIHZhbHVlICE9ICdzdHJpbmcnKSBmb3IodmFyIGtleSBpbiB2YWx1ZSkgX193ZWJwYWNrX3JlcXVpcmVfXy5kKG5zLCBrZXksIGZ1bmN0aW9uKGtleSkgeyByZXR1cm4gdmFsdWVba2V5XTsgfS5iaW5kKG51bGwsIGtleSkpO1xuIFx0XHRyZXR1cm4gbnM7XG4gXHR9O1xuXG4gXHQvLyBnZXREZWZhdWx0RXhwb3J0IGZ1bmN0aW9uIGZvciBjb21wYXRpYmlsaXR5IHdpdGggbm9uLWhhcm1vbnkgbW9kdWxlc1xuIFx0X193ZWJwYWNrX3JlcXVpcmVfXy5uID0gZnVuY3Rpb24obW9kdWxlKSB7XG4gXHRcdHZhciBnZXR0ZXIgPSBtb2R1bGUgJiYgbW9kdWxlLl9fZXNNb2R1bGUgP1xuIFx0XHRcdGZ1bmN0aW9uIGdldERlZmF1bHQoKSB7IHJldHVybiBtb2R1bGVbJ2RlZmF1bHQnXTsgfSA6XG4gXHRcdFx0ZnVuY3Rpb24gZ2V0TW9kdWxlRXhwb3J0cygpIHsgcmV0dXJuIG1vZHVsZTsgfTtcbiBcdFx0X193ZWJwYWNrX3JlcXVpcmVfXy5kKGdldHRlciwgJ2EnLCBnZXR0ZXIpO1xuIFx0XHRyZXR1cm4gZ2V0dGVyO1xuIFx0fTtcblxuIFx0Ly8gT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsXG4gXHRfX3dlYnBhY2tfcmVxdWlyZV9fLm8gPSBmdW5jdGlvbihvYmplY3QsIHByb3BlcnR5KSB7IHJldHVybiBPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwob2JqZWN0LCBwcm9wZXJ0eSk7IH07XG5cbiBcdC8vIF9fd2VicGFja19wdWJsaWNfcGF0aF9fXG4gXHRfX3dlYnBhY2tfcmVxdWlyZV9fLnAgPSBcIlwiO1xuXG5cbiBcdC8vIExvYWQgZW50cnkgbW9kdWxlIGFuZCByZXR1cm4gZXhwb3J0c1xuIFx0cmV0dXJuIF9fd2VicGFja19yZXF1aXJlX18oX193ZWJwYWNrX3JlcXVpcmVfXy5zID0gMCk7XG4iLCJhbmd1bGFyLm1vZHVsZSgndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlJylcclxuLmNvbnRyb2xsZXIoJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5jYXRhbG9nQ1NWZXhwb3J0Q29udHJvbGxlcicsIFsnJHNjb3BlJywgJ3BsYXRmb3JtV2ViQXBwLmJsYWRlTmF2aWdhdGlvblNlcnZpY2UnLCAndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmV4cG9ydCcsICd2aXJ0b0NvbW1lcmNlLmludmVudG9yeU1vZHVsZS5mdWxmaWxsbWVudHMnLCAndmlydG9Db21tZXJjZS5wcmljaW5nTW9kdWxlLnByaWNlbGlzdHMnLFxyXG4gICAgZnVuY3Rpb24gKCRzY29wZSwgYmxhZGVOYXZpZ2F0aW9uU2VydmljZSwgZXhwb3J0UmVzb3Vyc2UsIGZ1bGZpbGxtZW50cywgcHJpY2VsaXN0cykge1xyXG5cclxuICAgICAgICBjb25zdCBibGFkZSA9ICRzY29wZS5ibGFkZTtcclxuICAgICAgICBibGFkZS5mdWxmaWxtZW50Q2VudGVySWQgPSB1bmRlZmluZWQ7XHJcbiAgICAgICAgYmxhZGUucHJpY2VsaXN0SWQgPSB1bmRlZmluZWQ7XHJcbiAgICAgICAgYmxhZGUuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgYmxhZGUudGl0bGUgPSAnY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5ibGFkZXMuY2F0YWxvZy1DU1YtZXhwb3J0LnRpdGxlJztcclxuICAgICAgICBibGFkZS50aXRsZVZhbHVlcyA9IHsgbmFtZTogYmxhZGUuY2F0YWxvZyA/IGJsYWRlLmNhdGFsb2cubmFtZSA6ICcnIH07XHJcblxyXG4gICAgICAgIGZ1bmN0aW9uIGluaXRpYWxpemVCbGFkZSgpIHtcclxuICAgICAgICAgICAgZnVsZmlsbG1lbnRzLnNlYXJjaCh7fSwgZnVuY3Rpb24gKGRhdGEpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZihkYXRhLnRvdGFsQ291bnQgPiAwKXtcclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmZ1bGZpbGxtZW50Q2VudGVycyA9IGRhdGEucmVzdWx0cztcclxuICAgICAgICAgICAgICAgICAgICAgICAgYmxhZGUuZnVsZmlsbWVudENlbnRlcklkID0gXy5maXJzdChkYXRhLnJlc3VsdHMpLmlkO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICBmdW5jdGlvbiAoZXJyb3IpIHsgYmxhZGVOYXZpZ2F0aW9uU2VydmljZS5zZXRFcnJvcignRXJyb3IgJyArIGVycm9yLnN0YXR1cywgJHNjb3BlLmJsYWRlKTsgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAkc2NvcGUuJG9uKFwibmV3LW5vdGlmaWNhdGlvbi1ldmVudFwiLCBmdW5jdGlvbiAoZXZlbnQsIG5vdGlmaWNhdGlvbikge1xyXG4gICAgICAgICAgICBpZiAoYmxhZGUubm90aWZpY2F0aW9uICYmIG5vdGlmaWNhdGlvbi5pZCA9PT0gYmxhZGUubm90aWZpY2F0aW9uLmlkKSB7XHJcbiAgICAgICAgICAgICAgICBhbmd1bGFyLmNvcHkobm90aWZpY2F0aW9uLCBibGFkZS5ub3RpZmljYXRpb24pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICRzY29wZS5zdGFydEV4cG9ydCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgZXhwb3J0UmVzb3Vyc2UucnVuKHtcclxuICAgICAgICAgICAgICAgIGNhdGFsb2dJZDogYmxhZGUuY2F0YWxvZy5pZCxcclxuICAgICAgICAgICAgICAgIGNhdGVnb3J5SWRzOiBfLm1hcChibGFkZS5zZWxlY3RlZENhdGVnb3JpZXMsIGZ1bmN0aW9uICh4KSB7IHJldHVybiB4LmlkIH0pLFxyXG4gICAgICAgICAgICAgICAgcHJvZHVjdElkczogXy5tYXAoYmxhZGUuc2VsZWN0ZWRQcm9kdWN0cywgZnVuY3Rpb24gKHgpIHsgcmV0dXJuIHguaWQgfSksXHJcbiAgICAgICAgICAgICAgICBmdWxmaWxtZW50Q2VudGVySWQ6IGJsYWRlLmZ1bGZpbG1lbnRDZW50ZXJJZCxcclxuICAgICAgICAgICAgICAgIHByaWNlbGlzdElkOiBibGFkZS5wcmljZWxpc3RJZFxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBmdW5jdGlvbiAoZGF0YSkgeyBibGFkZS5ub3RpZmljYXRpb24gPSBkYXRhOyB9LFxyXG4gICAgICAgICAgICBmdW5jdGlvbiAoZXJyb3IpIHsgYmxhZGVOYXZpZ2F0aW9uU2VydmljZS5zZXRFcnJvcignRXJyb3IgJyArIGVycm9yLnN0YXR1cywgJHNjb3BlLmJsYWRlKTsgfSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgJHNjb3BlLnNldEZvcm0gPSBmdW5jdGlvbiAoZm9ybSkge1xyXG4gICAgICAgICAgICAkc2NvcGUuZm9ybVNjb3BlID0gZm9ybTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICBwcmljZWxpc3RzLnNlYXJjaCh7IHRha2U6IDEwMDAgfSwgZnVuY3Rpb24gKHJlc3VsdCkge1xyXG4gICAgICAgICAgICAkc2NvcGUucHJpY2VsaXN0cyA9IF8uZmlsdGVyKHJlc3VsdC5yZXN1bHRzLFxyXG4gICAgICAgICAgICAgICAgZnVuY3Rpb24gKHgpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gXy5zb21lKHguYXNzaWdubWVudHMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZ1bmN0aW9uICh5KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4geS5jYXRhbG9nSWQgPT09IGJsYWRlLmNhdGFsb2cuaWRcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICAkc2NvcGUuYmxhZGUuaGVhZEljb24gPSAnZmEtZmlsZS1hcmNoaXZlLW8nO1xyXG5cclxuICAgICAgICBpbml0aWFsaXplQmxhZGUoKTtcclxuICAgIH1dKTtcclxuIiwi77u/YW5ndWxhci5tb2R1bGUoJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZScpXHJcbi5jb250cm9sbGVyKCd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuY2F0YWxvZ0NTVmltcG9ydENvbnRyb2xsZXInLCBbJyRzY29wZScsICdwbGF0Zm9ybVdlYkFwcC5ibGFkZU5hdmlnYXRpb25TZXJ2aWNlJywgJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5pbXBvcnQnLCAncGxhdGZvcm1XZWJBcHAubm90aWZpY2F0aW9ucycsXHJcbiAgICBmdW5jdGlvbiAoJHNjb3BlLCBibGFkZU5hdmlnYXRpb25TZXJ2aWNlLCBpbXBvcnRSZXNvdXJzZSwgbm90aWZpY2F0aW9uc1Jlc291cmNlKSB7XHJcblxyXG4gICAgICAgIHZhciBibGFkZSA9ICRzY29wZS5ibGFkZTtcclxuICAgICAgICBibGFkZS5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICBibGFkZS50aXRsZSA9ICdjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmJsYWRlcy5jYXRhbG9nLUNTVi1pbXBvcnQudGl0bGUnO1xyXG5cclxuICAgICAgICAkc2NvcGUuJG9uKFwibmV3LW5vdGlmaWNhdGlvbi1ldmVudFwiLCBmdW5jdGlvbiAoZXZlbnQsIG5vdGlmaWNhdGlvbikge1xyXG4gICAgICAgICAgICBpZiAoYmxhZGUubm90aWZpY2F0aW9uICYmIG5vdGlmaWNhdGlvbi5pZCA9PSBibGFkZS5ub3RpZmljYXRpb24uaWQpIHtcclxuICAgICAgICAgICAgICAgIGFuZ3VsYXIuY29weShub3RpZmljYXRpb24sIGJsYWRlLm5vdGlmaWNhdGlvbik7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgJHNjb3BlLnNldEZvcm0gPSBmdW5jdGlvbiAoZm9ybSkge1xyXG4gICAgICAgICAgICAkc2NvcGUuZm9ybVNjb3BlID0gZm9ybTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgICRzY29wZS5ibGFkZUhlYWRJY28gPSAnZmEgZmEtZmlsZS1hcmNoaXZlLW8nO1xyXG5cclxuXHJcbiAgICB9XSk7XHJcbiIsIu+7v2FuZ3VsYXIubW9kdWxlKCd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUnKVxyXG4uY29udHJvbGxlcigndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmNhdGFsb2dDU1ZpbXBvcnRXaXphcmRNYXBwaW5nU3RlcENvbnRyb2xsZXInLCBbJyRzY29wZScsICdwbGF0Zm9ybVdlYkFwcC5ibGFkZU5hdmlnYXRpb25TZXJ2aWNlJywgZnVuY3Rpb24gKCRzY29wZSwgYmxhZGVOYXZpZ2F0aW9uU2VydmljZSkge1xyXG5cdHZhciBibGFkZSA9ICRzY29wZS5ibGFkZTtcclxuXHRcclxuXHJcblx0Ly9OZWVkIGF1dG9tYXRpY2FsbHkgdHJhY2tpbmcgXHJcblx0YW5ndWxhci5mb3JFYWNoKCRzY29wZS5ibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLnByb3BlcnR5TWFwcywgZnVuY3Rpb24gKHgpIHtcclxuXHRcdCRzY29wZS4kd2F0Y2goZnVuY3Rpb24gKCRzY29wZSkgeyByZXR1cm4geC5jc3ZDb2x1bW5OYW1lIH0sIGZ1bmN0aW9uIChuZXdWYWx1ZSwgb2xkVmFsdWUpIHtcclxuXHRcdFx0aWYgKG9sZFZhbHVlICE9IG5ld1ZhbHVlKSB7XHJcblx0XHRcdFx0aWYgKG5ld1ZhbHVlKSB7XHJcblx0XHRcdFx0XHR2YXIgaW5kZXggPSBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLnByb3BlcnR5Q3N2Q29sdW1ucy5pbmRleE9mKG5ld1ZhbHVlKTtcclxuXHRcdFx0XHRcdGlmIChpbmRleCAhPSAtMSkge1xyXG5cdFx0XHRcdFx0XHRibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLnByb3BlcnR5Q3N2Q29sdW1ucy5zcGxpY2UoaW5kZXgsIDEpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZiAob2xkVmFsdWUpIHtcclxuXHRcdFx0XHRcdHZhciBpbmRleCA9IGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24ucHJvcGVydHlDc3ZDb2x1bW5zLmluZGV4T2Yob2xkVmFsdWUpO1xyXG5cdFx0XHRcdFx0aWYgKGluZGV4ID09IC0xKSB7XHJcblx0XHRcdFx0XHRcdGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24ucHJvcGVydHlDc3ZDb2x1bW5zLnB1c2gob2xkVmFsdWUpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0fSk7XHJcblx0fSk7XHJcblxyXG5cdCRzY29wZS5jbGVhclByb3BlcnR5Q3N2Q29sdW1ucyA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24ucHJvcGVydHlDc3ZDb2x1bW5zLmxlbmd0aCA9IDA7XHJcblxyXG5cdH07XHJcblxyXG5cdCRzY29wZS5yZW1vdmVQcm9wZXJ0eUNzdkNvbHVtbiA9IGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuXHRcdHZhciBpbmRleCA9IGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24ucHJvcGVydHlDc3ZDb2x1bW5zLmluZGV4T2YoY29sdW1uKTtcclxuXHRcdGlmIChpbmRleCAhPSAtMSkge1xyXG5cdFx0XHRibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLnByb3BlcnR5Q3N2Q29sdW1ucy5zcGxpY2UoaW5kZXgsIDEpO1xyXG5cdFx0fVxyXG5cdH07XHJcblxyXG4gICAgJHNjb3BlLnNldEZvcm0gPSBmdW5jdGlvbiAoZm9ybSkge1xyXG4gICAgICAgICRzY29wZS5mb3JtU2NvcGUgPSBmb3JtO1xyXG4gICAgfVxyXG5cclxuICAgICRzY29wZS5pc1ZhbGlkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgXHRyZXR1cm4gJHNjb3BlLmZvcm1TY29wZSAmJiAkc2NvcGUuZm9ybVNjb3BlLiR2YWxpZDtcclxuICAgIH07XHJcblxyXG4gICAgJHNjb3BlLnNhdmVDaGFuZ2VzID0gZnVuY3Rpb24gKCkge1xyXG4gICAgXHQkc2NvcGUuYmxhZGVDbG9zZSgpO1xyXG4gICAgfTtcclxuXHJcbiAgICBibGFkZS5pc0xvYWRpbmcgPSBmYWxzZTtcclxufV0pO1xyXG5cclxuXHJcbiIsIu+7v2FuZ3VsYXIubW9kdWxlKCd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUnKVxyXG4uY29udHJvbGxlcigndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmNhdGFsb2dDU1ZpbXBvcnRXaXphcmRDb250cm9sbGVyJywgWyckc2NvcGUnLCAnJGxvY2FsU3RvcmFnZScsICdwbGF0Zm9ybVdlYkFwcC5ibGFkZU5hdmlnYXRpb25TZXJ2aWNlJywgJ0ZpbGVVcGxvYWRlcicsICd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuaW1wb3J0JyxcclxuICAgIGZ1bmN0aW9uICgkc2NvcGUsICRsb2NhbFN0b3JhZ2UsIGJsYWRlTmF2aWdhdGlvblNlcnZpY2UsIEZpbGVVcGxvYWRlciwgaW1wb3J0UmVzb3VyY2UpIHtcclxuXHJcbiAgICAgICAgdmFyIGJsYWRlID0gJHNjb3BlLmJsYWRlO1xyXG4gICAgICAgIGJsYWRlLmlzTG9hZGluZyA9IGZhbHNlO1xyXG4gICAgICAgIGJsYWRlLnRpdGxlID0gJ2NhdGFsb2dDc3ZJbXBvcnRNb2R1bGUud2l6YXJkcy5jYXRhbG9nLUNTVi1pbXBvcnQudGl0bGUnO1xyXG4gICAgICAgIGJsYWRlLnN1YnRpdGxlID0gJ2NhdGFsb2dDc3ZJbXBvcnRNb2R1bGUud2l6YXJkcy5jYXRhbG9nLUNTVi1pbXBvcnQuc3VidGl0bGUnO1xyXG4gICAgICAgIGJsYWRlLnN1YnRpdGxlVmFsZXMgPSB7IG5hbWU6IGJsYWRlLmNhdGFsb2cubmFtZSB9O1xyXG5cclxuICAgICAgICAkc2NvcGUuY29sdW1uRGVsaW1pdGVycyA9IFtcclxuICAgICAgICAgICAgeyBuYW1lOiBcImNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUud2l6YXJkcy5jYXRhbG9nLUNTVi1pbXBvcnQubGFiZWxzLnNwYWNlXCIsIHZhbHVlOiBcIiBcIiB9LFxyXG4gICAgICAgICAgICB7IG5hbWU6IFwiY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS53aXphcmRzLmNhdGFsb2ctQ1NWLWltcG9ydC5sYWJlbHMuY29tbWFcIiwgdmFsdWU6IFwiLFwiIH0sXHJcbiAgICAgICAgICAgIHsgbmFtZTogXCJjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLndpemFyZHMuY2F0YWxvZy1DU1YtaW1wb3J0LmxhYmVscy5zZW1pY29sb25cIiwgdmFsdWU6IFwiO1wiIH0sXHJcbiAgICAgICAgICAgIHsgbmFtZTogXCJjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLndpemFyZHMuY2F0YWxvZy1DU1YtaW1wb3J0LmxhYmVscy50YWJcIiwgdmFsdWU6IFwiXFx0XCIgfVxyXG4gICAgICAgIF07XHJcblxyXG4gICAgICAgIGlmICghJHNjb3BlLnVwbG9hZGVyKSB7XHJcbiAgICAgICAgICAgIC8vIGNyZWF0ZSB0aGUgdXBsb2FkZXJcclxuICAgICAgICAgICAgdmFyIHVwbG9hZGVyID0gJHNjb3BlLnVwbG9hZGVyID0gbmV3IEZpbGVVcGxvYWRlcih7XHJcbiAgICAgICAgICAgICAgICBzY29wZTogJHNjb3BlLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczogeyBBY2NlcHQ6ICdhcHBsaWNhdGlvbi9qc29uJyB9LFxyXG4gICAgICAgICAgICAgICAgdXJsOiAnYXBpL3BsYXRmb3JtL2Fzc2V0cz9mb2xkZXJVcmw9dG1wJyxcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogJ1BPU1QnLFxyXG4gICAgICAgICAgICAgICAgYXV0b1VwbG9hZDogdHJ1ZSxcclxuICAgICAgICAgICAgICAgIHJlbW92ZUFmdGVyVXBsb2FkOiB0cnVlXHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgLy8gQURESU5HIEZJTFRFUlM6IGNzdiBvbmx5XHJcbiAgICAgICAgICAgIC8vdXBsb2FkZXIuZmlsdGVycy5wdXNoKHtcclxuICAgICAgICAgICAgLy8gICAgbmFtZTogJ2NzdkZpbHRlcicsXHJcbiAgICAgICAgICAgIC8vICAgIGZuOiBmdW5jdGlvbiAoaSAvKntGaWxlfEZpbGVMaWtlT2JqZWN0fSovLCBvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIC8vICAgICAgICB2YXIgdHlwZSA9ICd8JyArIGkudHlwZS5zbGljZShpLnR5cGUubGFzdEluZGV4T2YoJy8nKSArIDEpICsgJ3wnO1xyXG4gICAgICAgICAgICAvLyAgICAgICAgcmV0dXJuICd8Y3N2fHZuZC5tcy1leGNlbHwnLmluZGV4T2YodHlwZSkgIT09IC0xO1xyXG4gICAgICAgICAgICAvLyAgICB9XHJcbiAgICAgICAgICAgIC8vfSk7XHJcblxyXG4gICAgICAgICAgICB1cGxvYWRlci5vbkJlZm9yZVVwbG9hZEl0ZW0gPSBmdW5jdGlvbiAoZmlsZUl0ZW0pIHtcclxuICAgICAgICAgICAgICAgIGJsYWRlLmlzTG9hZGluZyA9IHRydWU7XHJcbiAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICB1cGxvYWRlci5vblN1Y2Nlc3NJdGVtID0gZnVuY3Rpb24gKGZpbGVJdGVtLCBhc3NldCwgc3RhdHVzLCBoZWFkZXJzKSB7XHJcbiAgICAgICAgICAgICAgICBibGFkZS5jc3ZGaWxlVXJsID0gYXNzZXRbMF0ucmVsYXRpdmVVcmw7XHJcblxyXG4gICAgICAgICAgICAgICAgaW1wb3J0UmVzb3VyY2UuZ2V0TWFwcGluZ0NvbmZpZ3VyYXRpb24oeyBmaWxlVXJsOiBibGFkZS5jc3ZGaWxlVXJsLCBkZWxpbWl0ZXI6IGVuY29kZVVSSUNvbXBvbmVudChibGFkZS5jb2x1bW5EZWxpbWl0ZXIpIH0sIGZ1bmN0aW9uIChkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCRsb2NhbFN0b3JhZ2UubGFzdEtub3duSW1wb3J0RGF0YSAmJiAkbG9jYWxTdG9yYWdlLmxhc3RLbm93bkltcG9ydERhdGEuZVRhZyA9PT0gZGF0YS5lVGFnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGFuZ3VsYXIuZXh0ZW5kKGRhdGEsICRsb2NhbFN0b3JhZ2UubGFzdEtub3duSW1wb3J0RGF0YSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uID0gZGF0YTtcclxuICAgICAgICAgICAgICAgICAgICBibGFkZS5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgIH0sIGZ1bmN0aW9uIChlcnJvcikge1xyXG4gICAgICAgICAgICAgICAgICAgIGJsYWRlTmF2aWdhdGlvblNlcnZpY2Uuc2V0RXJyb3IoJ0Vycm9yICcgKyBlcnJvci5zdGF0dXMsIGJsYWRlKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgdXBsb2FkZXIub25BZnRlckFkZGluZ0FsbCA9IGZ1bmN0aW9uIChhZGRlZEl0ZW1zKSB7XHJcbiAgICAgICAgICAgICAgICBibGFkZU5hdmlnYXRpb25TZXJ2aWNlLnNldEVycm9yKG51bGwsIGJsYWRlKTtcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHVwbG9hZGVyLm9uRXJyb3JJdGVtID0gZnVuY3Rpb24gKGl0ZW0sIHJlc3BvbnNlLCBzdGF0dXMsIGhlYWRlcnMpIHtcclxuICAgICAgICAgICAgICAgIGJsYWRlTmF2aWdhdGlvblNlcnZpY2Uuc2V0RXJyb3IoaXRlbS5fZmlsZS5uYW1lICsgJyBmYWlsZWQ6ICcgKyAocmVzcG9uc2UubWVzc2FnZSA/IHJlc3BvbnNlLm1lc3NhZ2UgOiBzdGF0dXMpLCBibGFkZSk7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgJHNjb3BlLmNhbk1hcENvbHVtbnMgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uICYmICRzY29wZS5mb3JtU2NvcGUgJiYgJHNjb3BlLmZvcm1TY29wZS4kdmFsaWQ7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAkc2NvcGUub3Blbk1hcHBpbmdTdGVwID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgbmV3QmxhZGUgPSB7XHJcbiAgICAgICAgICAgICAgICBpZDogXCJpbXBvcnRNYXBwaW5nXCIsXHJcbiAgICAgICAgICAgICAgICBpbXBvcnRDb25maWd1cmF0aW9uOiBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLFxyXG4gICAgICAgICAgICAgICAgdGl0bGU6ICdjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLndpemFyZHMuY2F0YWxvZy1DU1YtaW1wb3J0LXdpemFyZC1tYXBwaW5nLXN0ZXAudGl0bGUnLFxyXG4gICAgICAgICAgICAgICAgc3VidGl0bGU6ICdjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLndpemFyZHMuY2F0YWxvZy1DU1YtaW1wb3J0LXdpemFyZC1tYXBwaW5nLXN0ZXAuc3VidGl0bGUnLFxyXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5jYXRhbG9nQ1NWaW1wb3J0V2l6YXJkTWFwcGluZ1N0ZXBDb250cm9sbGVyJyxcclxuICAgICAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvaW1wb3J0L3dpemFyZC9jYXRhbG9nLUNTVi1pbXBvcnQtd2l6YXJkLW1hcHBpbmctc3RlcC50cGwuaHRtbCdcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIGJsYWRlLmNhbkltcG9ydCA9IHRydWU7XHJcbiAgICAgICAgICAgIGJsYWRlTmF2aWdhdGlvblNlcnZpY2Uuc2hvd0JsYWRlKG5ld0JsYWRlLCBibGFkZSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgJHNjb3BlLnN0YXJ0SW1wb3J0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAkbG9jYWxTdG9yYWdlLmxhc3RLbm93bkltcG9ydERhdGEgPSB7XHJcbiAgICAgICAgICAgICAgICBlVGFnOiBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLmVUYWcsXHJcbiAgICAgICAgICAgICAgICBwcm9wZXJ0eU1hcHM6IGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24ucHJvcGVydHlNYXBzLFxyXG4gICAgICAgICAgICAgICAgcHJvcGVydHlDc3ZDb2x1bW5zOiBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLnByb3BlcnR5Q3N2Q29sdW1uc1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgdmFyIGV4cG9ydEluZm8gPSB7IGNvbmZpZ3VyYXRpb246IGJsYWRlLmltcG9ydENvbmZpZ3VyYXRpb24sIGZpbGVVcmw6IGJsYWRlLmNzdkZpbGVVcmwsIGNhdGFsb2dJZDogYmxhZGUuY2F0YWxvZy5pZCB9O1xyXG4gICAgICAgICAgICBpbXBvcnRSZXNvdXJjZS5ydW4oZXhwb3J0SW5mbywgZnVuY3Rpb24gKG5vdGlmaWNhdGlvbikge1xyXG4gICAgICAgICAgICAgICAgdmFyIG5ld0JsYWRlID0ge1xyXG4gICAgICAgICAgICAgICAgICAgIGlkOiBcImltcG9ydFByb2dyZXNzXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgY2F0YWxvZzogYmxhZGUuY2F0YWxvZyxcclxuICAgICAgICAgICAgICAgICAgICBub3RpZmljYXRpb246IG5vdGlmaWNhdGlvbixcclxuICAgICAgICAgICAgICAgICAgICBpbXBvcnRDb25maWd1cmF0aW9uOiBibGFkZS5pbXBvcnRDb25maWd1cmF0aW9uLFxyXG4gICAgICAgICAgICAgICAgICAgIGNvbnRyb2xsZXI6ICd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuY2F0YWxvZ0NTVmltcG9ydENvbnRyb2xsZXInLFxyXG4gICAgICAgICAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvaW1wb3J0L2NhdGFsb2ctQ1NWLWltcG9ydC50cGwuaHRtbCdcclxuICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgJHNjb3BlLiRvbihcIm5ldy1ub3RpZmljYXRpb24tZXZlbnRcIiwgZnVuY3Rpb24gKGV2ZW50LCBub3RpZmljYXRpb24pIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAobm90aWZpY2F0aW9uICYmIG5vdGlmaWNhdGlvbi5pZCA9PSBuZXdCbGFkZS5ub3RpZmljYXRpb24uaWQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYmxhZGUuY2FuSW1wb3J0ID0gbm90aWZpY2F0aW9uLmZpbmlzaGVkICE9IG51bGw7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgYmxhZGUuY2FuSW1wb3J0ID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICBibGFkZU5hdmlnYXRpb25TZXJ2aWNlLnNob3dCbGFkZShuZXdCbGFkZSwgYmxhZGUpO1xyXG5cclxuICAgICAgICAgICAgfSwgZnVuY3Rpb24gKGVycm9yKSB7XHJcbiAgICAgICAgICAgICAgICBibGFkZU5hdmlnYXRpb25TZXJ2aWNlLnNldEVycm9yKCdFcnJvciAnICsgZXJyb3Iuc3RhdHVzLCBibGFkZSk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgICRzY29wZS5zZXRGb3JtID0gZnVuY3Rpb24gKGZvcm0pIHtcclxuICAgICAgICAgICAgJHNjb3BlLmZvcm1TY29wZSA9IGZvcm07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICB9XSk7XHJcbiIsIu+7v2FuZ3VsYXIubW9kdWxlKCd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUnKVxyXG4uZmFjdG9yeSgndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLldlYkFwaScsIFsnJHJlc291cmNlJywgZnVuY3Rpb24gKCRyZXNvdXJjZSkge1xyXG4gICAgcmV0dXJuICRyZXNvdXJjZSgnYXBpL2NhdGFsb2djc3ZpbXBvcnQnKTtcclxufV0pO1xyXG4iLCLvu79hbmd1bGFyLm1vZHVsZSgndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlJylcclxuLmZhY3RvcnkoJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5leHBvcnQnLCBbJyRyZXNvdXJjZScsIGZ1bmN0aW9uICgkcmVzb3VyY2UpIHtcclxuXHJcbiAgICByZXR1cm4gJHJlc291cmNlKCdhcGkvY2F0YWxvZ2NzdmltcG9ydC9leHBvcnQvOmlkJywgeyBpZDogJ0BpZCcgfSwge1xyXG4gICAgICAgIHJ1bjogeyBtZXRob2Q6ICdQT1NUJywgdXJsOiAnYXBpL2NhdGFsb2djc3ZpbXBvcnQvZXhwb3J0JywgaXNBcnJheTogZmFsc2UgfVxyXG4gICAgfSk7XHJcblxyXG59XSk7Iiwi77u/YW5ndWxhci5tb2R1bGUoJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZScpXHJcbi5mYWN0b3J5KCd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuaW1wb3J0JywgWyckcmVzb3VyY2UnLCBmdW5jdGlvbiAoJHJlc291cmNlKSB7XHJcblxyXG4gICAgcmV0dXJuICRyZXNvdXJjZSgnYXBpL2NhdGFsb2djc3ZpbXBvcnQvaW1wb3J0Jywge30sIHtcclxuICAgICAgICBnZXRNYXBwaW5nQ29uZmlndXJhdGlvbjogeyBtZXRob2Q6ICdHRVQnLCB1cmw6ICdhcGkvY2F0YWxvZ2NzdmltcG9ydC9pbXBvcnQvbWFwcGluZ2NvbmZpZ3VyYXRpb24nLCBpc0FycmF5OiBmYWxzZSB9LFxyXG4gICAgICAgIHJ1bjogeyBtZXRob2Q6ICdQT1NUJywgdXJsOiAnYXBpL2NhdGFsb2djc3ZpbXBvcnQvaW1wb3J0JywgaXNBcnJheTogZmFsc2UgfVxyXG5cdH0pO1xyXG5cclxufV0pOyIsIi8vQ2FsbCB0aGlzIHRvIHJlZ2lzdGVyIG91ciBtb2R1bGUgdG8gbWFpbiBhcHBsaWNhdGlvblxyXG52YXIgbW9kdWxlVGVtcGxhdGVOYW1lID0gXCJ2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGVcIjtcclxuXHJcbmlmIChBcHBEZXBlbmRlbmNpZXMgIT0gdW5kZWZpbmVkKSB7XHJcbiAgICBBcHBEZXBlbmRlbmNpZXMucHVzaChtb2R1bGVUZW1wbGF0ZU5hbWUpO1xyXG59XHJcblxyXG5hbmd1bGFyLm1vZHVsZShtb2R1bGVUZW1wbGF0ZU5hbWUsIFtdKVxyXG4uY29uZmlnKFsnJHN0YXRlUHJvdmlkZXInLCAnJHVybFJvdXRlclByb3ZpZGVyJyxcclxuICAgIGZ1bmN0aW9uICgkc3RhdGVQcm92aWRlciwgJHVybFJvdXRlclByb3ZpZGVyKSB7XHJcbiAgICB9XHJcbl0pXHJcbiAgICAucnVuKFsnJHJvb3RTY29wZScsICdwbGF0Zm9ybVdlYkFwcC5tYWluTWVudVNlcnZpY2UnLCAncGxhdGZvcm1XZWJBcHAud2lkZ2V0U2VydmljZScsICdwbGF0Zm9ybVdlYkFwcC50b29sYmFyU2VydmljZScsICdwbGF0Zm9ybVdlYkFwcC5wdXNoTm90aWZpY2F0aW9uVGVtcGxhdGVSZXNvbHZlcicsICdwbGF0Zm9ybVdlYkFwcC5ibGFkZU5hdmlnYXRpb25TZXJ2aWNlJywgJyRzdGF0ZScsICd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dNb2R1bGUuY2F0YWxvZ0ltcG9ydFNlcnZpY2UnLCAndmlydG9Db21tZXJjZS5jYXRhbG9nTW9kdWxlLmNhdGFsb2dFeHBvcnRTZXJ2aWNlJyxcclxuICAgICAgICBmdW5jdGlvbiAoJHJvb3RTY29wZSwgbWFpbk1lbnVTZXJ2aWNlLCB3aWRnZXRTZXJ2aWNlLCB0b29sYmFyU2VydmljZSwgcHVzaE5vdGlmaWNhdGlvblRlbXBsYXRlUmVzb2x2ZXIsIGJsYWRlTmF2aWdhdGlvblNlcnZpY2UsICRzdGF0ZSwgY2F0YWxvZ0ltcG9ydFNlcnZpY2UsIGNhdGFsb2dFeHBvcnRTZXJ2aWNlKSB7XHJcblxyXG4gICAgICAgIC8vTk9USUZJQ0FUSU9OU1xyXG4gICAgICAgIC8vRXhwb3J0XHJcbiAgICAgICAgdmFyIG1lbnVFeHBvcnRUZW1wbGF0ZSA9XHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBwcmlvcml0eTogOTAwLFxyXG4gICAgICAgICAgICBzYXRpc2Z5OiBmdW5jdGlvbiAobm90aWZ5LCBwbGFjZSkgeyByZXR1cm4gcGxhY2UgPT0gJ21lbnUnICYmIG5vdGlmeS5ub3RpZnlUeXBlID09ICdDYXRhbG9nQ3N2RXhwb3J0JzsgfSxcclxuICAgICAgICAgICAgdGVtcGxhdGU6ICdNb2R1bGVzLyQoVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0TW9kdWxlKS9TY3JpcHRzL2JsYWRlcy9leHBvcnQvbm90aWZpY2F0aW9ucy9tZW51RXhwb3J0LnRwbC5odG1sJyxcclxuICAgICAgICAgICAgYWN0aW9uOiBmdW5jdGlvbiAobm90aWZ5KSB7ICRzdGF0ZS5nbygnd29ya3NwYWNlLnB1c2hOb3RpZmljYXRpb25zSGlzdG9yeScsIG5vdGlmeSkgfVxyXG4gICAgICAgIH07XHJcbiAgICAgICAgcHVzaE5vdGlmaWNhdGlvblRlbXBsYXRlUmVzb2x2ZXIucmVnaXN0ZXIobWVudUV4cG9ydFRlbXBsYXRlKTtcclxuXHJcbiAgICAgICAgdmFyIGhpc3RvcnlFeHBvcnRUZW1wbGF0ZSA9XHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBwcmlvcml0eTogOTAwLFxyXG4gICAgICAgICAgICBzYXRpc2Z5OiBmdW5jdGlvbiAobm90aWZ5LCBwbGFjZSkgeyByZXR1cm4gcGxhY2UgPT0gJ2hpc3RvcnknICYmIG5vdGlmeS5ub3RpZnlUeXBlID09ICdDYXRhbG9nQ3N2RXhwb3J0JzsgfSxcclxuICAgICAgICAgICAgdGVtcGxhdGU6ICdNb2R1bGVzLyQoVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0TW9kdWxlKS9TY3JpcHRzL2JsYWRlcy9leHBvcnQvbm90aWZpY2F0aW9ucy9oaXN0b3J5RXhwb3J0LnRwbC5odG1sJyxcclxuICAgICAgICAgICAgYWN0aW9uOiBmdW5jdGlvbiAobm90aWZ5KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgYmxhZGUgPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaWQ6ICdDYXRhbG9nQ3N2RXhwb3J0RGV0YWlsJyxcclxuICAgICAgICAgICAgICAgICAgICB0aXRsZTogJ2NhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuYmxhZGVzLmhpc3RvcnkuZXhwb3J0LnRpdGxlJyxcclxuICAgICAgICAgICAgICAgICAgICBzdWJ0aXRsZTogJ2NhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuYmxhZGVzLmhpc3RvcnkuZXhwb3J0LnN1YnRpdGxlJyxcclxuICAgICAgICAgICAgICAgICAgICB0ZW1wbGF0ZTogJ01vZHVsZXMvJChWaXJ0b0NvbW1lcmNlLkNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUpL1NjcmlwdHMvYmxhZGVzL2V4cG9ydC9jYXRhbG9nLUNTVi1leHBvcnQudHBsLmh0bWwnLFxyXG4gICAgICAgICAgICAgICAgICAgIGNvbnRyb2xsZXI6ICd2aXJ0b0NvbW1lcmNlLmNhdGFsb2dDc3ZJbXBvcnRNb2R1bGUuY2F0YWxvZ0NTVmV4cG9ydENvbnRyb2xsZXInLFxyXG4gICAgICAgICAgICAgICAgICAgIG5vdGlmaWNhdGlvbjogbm90aWZ5XHJcbiAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAgICAgYmxhZGVOYXZpZ2F0aW9uU2VydmljZS5zaG93QmxhZGUoYmxhZGUpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuICAgICAgICBwdXNoTm90aWZpY2F0aW9uVGVtcGxhdGVSZXNvbHZlci5yZWdpc3RlcihoaXN0b3J5RXhwb3J0VGVtcGxhdGUpO1xyXG5cclxuICAgICAgICAvL0ltcG9ydFxyXG4gICAgICAgIHZhciBtZW51SW1wb3J0VGVtcGxhdGUgPVxyXG4gICAgICAgIHtcclxuICAgICAgICAgICAgcHJpb3JpdHk6IDkwMCxcclxuICAgICAgICAgICAgc2F0aXNmeTogZnVuY3Rpb24gKG5vdGlmeSwgcGxhY2UpIHsgcmV0dXJuIHBsYWNlID09ICdtZW51JyAmJiBub3RpZnkubm90aWZ5VHlwZSA9PSAnQ2F0YWxvZ0NzdkltcG9ydCc7IH0sXHJcbiAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvaW1wb3J0L25vdGlmaWNhdGlvbnMvbWVudUltcG9ydC50cGwuaHRtbCcsXHJcbiAgICAgICAgICAgIGFjdGlvbjogZnVuY3Rpb24gKG5vdGlmeSkgeyAkc3RhdGUuZ28oJ3dvcmtzcGFjZS5wdXNoTm90aWZpY2F0aW9uc0hpc3RvcnknLCBub3RpZnkpIH1cclxuICAgICAgICB9O1xyXG4gICAgICAgIHB1c2hOb3RpZmljYXRpb25UZW1wbGF0ZVJlc29sdmVyLnJlZ2lzdGVyKG1lbnVJbXBvcnRUZW1wbGF0ZSk7XHJcblxyXG4gICAgICAgIHZhciBoaXN0b3J5SW1wb3J0VGVtcGxhdGUgPVxyXG4gICAgICAgIHtcclxuICAgICAgICAgICAgcHJpb3JpdHk6IDkwMCxcclxuICAgICAgICAgICAgc2F0aXNmeTogZnVuY3Rpb24gKG5vdGlmeSwgcGxhY2UpIHsgcmV0dXJuIHBsYWNlID09ICdoaXN0b3J5JyAmJiBub3RpZnkubm90aWZ5VHlwZSA9PSAnQ2F0YWxvZ0NzdkltcG9ydCc7IH0sXHJcbiAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvaW1wb3J0L25vdGlmaWNhdGlvbnMvaGlzdG9yeUltcG9ydC50cGwuaHRtbCcsXHJcbiAgICAgICAgICAgIGFjdGlvbjogZnVuY3Rpb24gKG5vdGlmeSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGJsYWRlID0ge1xyXG4gICAgICAgICAgICAgICAgICAgIGlkOiAnQ2F0YWxvZ0NzdkltcG9ydERldGFpbCcsXHJcbiAgICAgICAgICAgICAgICAgICAgdGl0bGU6ICdjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmJsYWRlcy5oaXN0b3J5LmltcG9ydC50aXRsZScsXHJcbiAgICAgICAgICAgICAgICAgICAgc3VidGl0bGU6ICdjYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmJsYWRlcy5oaXN0b3J5LmltcG9ydC50aXRsZScsXHJcbiAgICAgICAgICAgICAgICAgICAgdGVtcGxhdGU6ICdNb2R1bGVzLyQoVmlydG9Db21tZXJjZS5DYXRhbG9nQ3N2SW1wb3J0TW9kdWxlKS9TY3JpcHRzL2JsYWRlcy9pbXBvcnQvY2F0YWxvZy1DU1YtaW1wb3J0LnRwbC5odG1sJyxcclxuICAgICAgICAgICAgICAgICAgICBjb250cm9sbGVyOiAndmlydG9Db21tZXJjZS5jYXRhbG9nQ3N2SW1wb3J0TW9kdWxlLmNhdGFsb2dDU1ZpbXBvcnRDb250cm9sbGVyJyxcclxuICAgICAgICAgICAgICAgICAgICBub3RpZmljYXRpb246IG5vdGlmeVxyXG4gICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgICAgIGJsYWRlTmF2aWdhdGlvblNlcnZpY2Uuc2hvd0JsYWRlKGJsYWRlKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcbiAgICAgICAgcHVzaE5vdGlmaWNhdGlvblRlbXBsYXRlUmVzb2x2ZXIucmVnaXN0ZXIoaGlzdG9yeUltcG9ydFRlbXBsYXRlKTtcclxuXHJcbiAgICAgICAgLy8gSU1QT1JUIC8gRVhQT1JUXHJcbiAgICAgICAgY2F0YWxvZ0ltcG9ydFNlcnZpY2UucmVnaXN0ZXIoe1xyXG4gICAgICAgICAgICBuYW1lOiAnVmlydG9Db21tZXJjZSBDU1YgaW1wb3J0JyxcclxuICAgICAgICAgICAgZGVzY3JpcHRpb246ICdOYXRpdmUgVmlydG9Db21tZXJjZSBjYXRhbG9nIGRhdGEgaW1wb3J0IGZyb20gQ1NWJyxcclxuICAgICAgICAgICAgaWNvbjogJ2ZhIGZhLWZpbGUtYXJjaGl2ZS1vJyxcclxuICAgICAgICAgICAgY29udHJvbGxlcjogJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5jYXRhbG9nQ1NWaW1wb3J0V2l6YXJkQ29udHJvbGxlcicsXHJcbiAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvaW1wb3J0L3dpemFyZC9jYXRhbG9nLUNTVi1pbXBvcnQtd2l6YXJkLnRwbC5odG1sJ1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBjYXRhbG9nRXhwb3J0U2VydmljZS5yZWdpc3Rlcih7XHJcbiAgICAgICAgICAgIG5hbWU6ICdWaXJ0b0NvbW1lcmNlIENTViBleHBvcnQnLFxyXG4gICAgICAgICAgICBkZXNjcmlwdGlvbjogJ05hdGl2ZSBWaXJ0b0NvbW1lcmNlIGNhdGFsb2cgZGF0YSBleHBvcnQgdG8gQ1NWJyxcclxuICAgICAgICAgICAgaWNvbjogJ2ZhIGZhLWZpbGUtYXJjaGl2ZS1vJyxcclxuICAgICAgICAgICAgY29udHJvbGxlcjogJ3ZpcnRvQ29tbWVyY2UuY2F0YWxvZ0NzdkltcG9ydE1vZHVsZS5jYXRhbG9nQ1NWZXhwb3J0Q29udHJvbGxlcicsXHJcbiAgICAgICAgICAgIHRlbXBsYXRlOiAnTW9kdWxlcy8kKFZpcnRvQ29tbWVyY2UuQ2F0YWxvZ0NzdkltcG9ydE1vZHVsZSkvU2NyaXB0cy9ibGFkZXMvZXhwb3J0L2NhdGFsb2ctQ1NWLWV4cG9ydC50cGwuaHRtbCdcclxuICAgICAgICB9KTtcclxuICAgIH1cclxuXSk7XHJcbiJdLCJzb3VyY2VSb290IjoiIn0=