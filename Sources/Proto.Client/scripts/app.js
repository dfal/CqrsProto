'use strict';

var protoApp = angular.module('protoApp', [
    'ngRoute',
    'protoControllers'
]);

protoApp.config(['$routeProvider',
    function ($routeProvider) {
        $routeProvider.
            when('/customers', {
                templateUrl: 'partials/customer-list.html',
                controller: 'CustomerListController'
            }).
            when('/customer/:customerId', {
                templateUrl: 'partials/customer-detail.html',
                controller: 'CustomerDetailController'
            }).
            otherwise({
                redirectTo: 'customers'
            });
    }]);