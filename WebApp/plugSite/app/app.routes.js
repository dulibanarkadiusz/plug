var app = angular.module('plugAppRouter', ['ui.router']);

app.config(function($stateProvider, $urlRouterProvider){

	$urlRouterProvider.otherwise('/home');

	$stateProvider
		.state('home',{
			url: '/home',
			templateUrl: 'app/views/home.html'
		})
		.state('energy',{
			url: '/energy',
			templateUrl: 'app/views/energyCalc.html'
		})

}).run(function($rootScope, $state) {
  	$rootScope.$state = $state;
});

app.filter('dayOfWeek', function() {
    return function(day) {
        switch(day){
        	case 0: return "Niedziela";
        	case 4: return "Czwartek";
        	default: return "Żodyn";
        }
    };
});