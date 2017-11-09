var app = angular.module("plugApp", [
		"angularMoment",
		"plugAppRouter",
		"plugAppMainCtrl",
		"ui.bootstrap"
]);

app.filter('dayNameFilter', function () {
    return function (day) {
        return "x";
    }
});
