'use strict';

var protoControllers = angular.module('protoControllers', []);

protoControllers.controller('CustomerListController', ['$scope', '$http',
    function ($scope, $http) {
        $http.get('customers/customers.json').success(function (data) {
            $scope.customers = data;
        });
        
        $scope.orderProp = 'name';
    }]);

protoControllers.controller('CustomerDetailController', ['$scope', '$routeParams',
    function ($scope, $routeParams) {
        $scope.customerId = $routeParams.customerId;
    }]);

