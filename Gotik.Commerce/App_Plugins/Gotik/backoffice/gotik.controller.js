angular.module("umbraco").controller("GotikCommerceDashboardController",
    function ($scope, $http) {
        console.log("Gotik Commerce Dashboard Loaded");

        $scope.stats = {
            loading: true
        };

        // You can add API calls here to fetch statistics
        // Example:
        // $http.get('/api/v1/products/count').then(function(response) {
        //     $scope.stats.productCount = response.data;
        // });

        $scope.stats.loading = false;
    }
);
