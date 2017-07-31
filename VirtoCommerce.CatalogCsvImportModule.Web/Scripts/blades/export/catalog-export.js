angular.module('virtoCommerce.catalogCsvImportModule')
    .factory('virtoCommerce.catalogCsvImportModule.catalogExportService', function () {
        var retVal = {
            registrationsList: [],
            register: function (registration) {
                this.registrationsList.push(registration);
            }
        };
        return retVal;
    });