angular.module('virtoCommerce.catalogCsvImportModule')
    .factory('virtoCommerce.catalogCsvImportModule.catalogImportService', function () {
        var retVal = {
            registrationsList: [],
            register: function (registration) {
                this.registrationsList.push(registration);
            }
        };
        return retVal;
    });