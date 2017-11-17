angular.module('plugAppPriceCtrl', []).controller('priceCtrl', function($scope, $state, $http, amMoment, $timeout, $uibModal){
	$scope.device = {};
	$scope.indicators = {};
	$scope.errorString = "";

	var modalInstance;
	var successCount = 0;
	var errorsCount = 0;
	var isCatched = false;

	$scope.price = 0.40;
	if (localStorage.price){
		$scope.price = parseFloat(localStorage.getItem("price"));
	}

	$scope.getEnergy = function(){
		$http({
			method: 'GET',
		 	url: apiUrl + 'CurrentRun'
		}).then(function successCallback(response) {
		    $scope.energy = response.data[0].EnergyValue;
		    //$scope.httpSuccess();
		    $scope.calc();
	  	}, function errorCallback(response) {
	  		//$scope.httpError();
		});
	}

	$scope.calc = function(){
		if (!isNaN(parseFloat($scope.price))){
			localStorage.setItem("price", $scope.price);
			$scope.costs = jQuery.extend({}, $scope.energy);
			$scope.costs.Day = parseFloat($scope.costs.Day * $scope.price).toFixed(2);
			$scope.costs.Week = parseFloat($scope.costs.Week * $scope.price).toFixed(2);
			$scope.costs.Month = parseFloat($scope.costs.Month * $scope.price).toFixed(2);
		}
		else{
			$scope.message = "Proszę podać poprawny koszt zakupu.";
			$scope.costs.Day = "-";
			$scope.costs.Week = "-";
			$scope.costs.Month = "-";
		}
	}

});