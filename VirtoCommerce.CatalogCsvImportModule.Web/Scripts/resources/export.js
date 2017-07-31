angular.module('virtoCommerce.catalogCsvImportModule')
.factory('virtoCommerce.catalogCsvImportModule.export', ['$resource', function ($resource) {

    return $resource('api/catalogcsvimport/export/:id', { id: '@id' }, {
        run: { method: 'POST', url: 'api/catalogcsvimport/export', isArray: false }
    });

}]);