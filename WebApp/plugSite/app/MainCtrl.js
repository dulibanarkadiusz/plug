var apiUrl = "http://localhost:8081/api/";
angular.module('plugAppMainCtrl', []).controller('mainCtrl', function($scope, $http, amMoment, $timeout, $uibModal){
	$scope.header = "test";
	$scope.device = {};
	$scope.indicators = {};
	$scope.errorString = "";

	var modalInstance;
	var successCount = 0;
	var errorsCount = 0;
	var isCatched = false;

	$scope.init = function(){
		modalInstance = $uibModal.open({
			animation: $scope.animationsEnabled,
			backdrop: 'static',
			templateUrl: 'app/views/waitModal.html'
		});

		modalInstance.result.then(function(){
		}, function(){
		});

		$scope.getStatus();
		$scope.getIndicators();
		$scope.getDeviceInfo();

		$timeout($scope.tryCancelInit, 100);
	}

	$scope.getDeviceInfo = function(){
		$http({
			method: 'GET',
		 	url: apiUrl + 'DeviceConfig'
		}).then(function successCallback(response) {
		    $scope.device = response.data[0];
		    $scope.httpSuccess();
	  	}, function errorCallback(response) {
	  		$scope.httpError();
		});
	}

	$scope.getIndicators = function(){
		$http({
			method: 'GET',
		 	url: apiUrl + 'CurrentRun'
		}).then(function successCallback(response) {
		    $scope.indicators = response.data[0];
		    $scope.indicators.CurrentValue = parseFloat($scope.indicators.CurrentValue).toFixed(3);
		    $scope.indicators.LastToogleTime = moment($scope.indicators.LastToogleTime).add(-10, 's');

		    $scope.httpSuccess();
	  	}, function errorCallback(response) {
	  		$scope.httpError();
		});
	}

	$scope.getStatus = function(){
		$http({
			method: 'GET',
		 	url: apiUrl + 'State'
		}).then(function successCallback(response) {
		    $scope.state = response.data[0];
		    $scope.httpSuccess();
	  	}, function errorCallback(response) {
	  		$scope.httpError();
		});
	}

	$scope.switchStatus = function(){
		$scope.isDataProccessing = true;
		$http({
			method: 'POST',
		 	url: apiUrl + 'State?isOn=' + $scope.state.IsOn
		}).then(function successCallback(response) {
		    $scope.state = response.data[0];
		    $scope.isDataProccessing = false;
		    $scope.getIndicators();
	  	}, function errorCallback(response) {
		    $scope.httpError();
		});
	}

	$scope.saveDeviceName = function(){
		$scope.isDataProccessing = true;
		$http({
			method: 'POST',
		 	url: apiUrl + 'DeviceConfig?name=' + $scope.device.Name
		}).then(function successCallback(response) {
		    $scope.editEnabled = false;
		    $scope.isDataProccessing = false;
	  	}, function errorCallback(response) {
		    $scope.httpError();
		});
	}

	$scope.getDeviceParams = function(){
		$scope.getStatus();
		$scope.getIndicators();
		$timeout($scope.getDeviceParams, 5000);
	}


	$scope.enableEdit = function(){
		$scope.editEnabled = true;
	}

	$scope.httpSuccess = function(){
		successCount++;
		$scope.errorString = "";
		modalInstance.dismiss('close');
	}

	$scope.httpError = function(){
		if (successCount > 3){
			$scope.errorString = "Utracono połączenie z urządzeniem!";
		}
		else if (!isCatched){
			modalInstance.dismiss('close');
			modalInstance = $uibModal.open({
				animation: $scope.animationsEnabled,
				backdrop: 'static',
				templateUrl: 'app/views/errorModal.html'
			});

			modalInstance.result.then(function(){
				}, function(){
			});

			isCatched = true;
		}
	}

	$scope.tryCancelInit = function(){
		if (successCount == 3){
			modalInstance.dismiss('close');
			$timeout($scope.getDeviceParams, 5000);
		}
		else if (errorsCount > 1){

		}
		else{
			$timeout($scope.tryCancelInit, 100);
		}
	}

});