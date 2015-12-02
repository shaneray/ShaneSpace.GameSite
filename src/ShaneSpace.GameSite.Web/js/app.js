var myApp = angular.module('gamesiteApp', ['ngRoute', 'ngCookies', 'gamesiteAppControllers', 'AuthService', 'camelCaseToHuman', 'angularMoment', 'ErrorService','snap','ui.bootstrap'])
     .config(config)
     .run(run)
    .filter('replaceDieWithImage', ['$sce', function ($sce) {
        return function (str) {
            return $sce.trustAsHtml(str.replace(/die and got the following values:/, 'die and got the following values:<br />')
                .replace(/D(\d+):(\d+),*/g, '<span style="position:relative;"><img class="rolledDieImage" src="../../images/die/D$1.gif"><span class="centered dieText"><strong>$2</strong><span style="font-size: 10px;"><br>D$1</span></span></span>'));
        };
    }])
    .directive("select", function() {
        return {
            restrict: "E",
            require: "?ngModel",
            scope: false,
            link: function (scope, element, attrs, ngModel) {
                if (!ngModel) {
                    return;
                }
                element.bind("keyup", function() {
                    element.triggerHandler("change");
                })
            }
        }
    })
    .directive('onLastRepeat', function () {
        return function (scope, element, attrs) {
            if (scope.$last) setTimeout(function () {
                scope.$emit('onRepeatLast', element, attrs);
            }, 1);
        };
    })
    .directive('loading', function () {
        var linkFunction = function (scope, element, attributes, ctrl, transclude) {
            scope.loading = false;
            scope.$watch(function () {
                return scope.$parent.$eval(attributes.loadingcondition);
            }, function (newVal) {
                scope.loading = newVal
            });
            angular.forEach(attributes, function (value, key) {
                if (key == 'loadingtext') { scope.loadingText = value; }
            });
            transclude(scope.$parent, function (content) {
                element.find("div").append(content);
            });
        };
        return {
            restrict: 'AE',
            replace: 'true',
            transclude: true,
            scope: {},
            link: linkFunction,
            template: '<div><div class="col-md-6 col-md-offset-3" ng-show="loading == true"><panel header="Loading"><span class="centered"><h2>{{loadingText}}</h2></span></panel></div>'
            + '<div ng-show="loading != true"></div></div>'
        };
    })
    .directive('panel', function () {
        var linkFunction = function (scope, element, attributes, ctrl, transclude) {
            scope.headerButton = false;
            angular.forEach(attributes, function (value, key) {
                if (key == 'header') { scope.header = value; }
                if (key == 'headerbuttontext') {
                    scope.headerButtonText = value;
                }
                if (key == 'headerbuttonlink') { scope.headerButtonLink = value; }
            });
            if (scope.headerButtonText !== undefined) {
                scope.headerButton = true
            }
            transclude(scope.$parent, function (content) {
                element.find("div").append(content);
            });
        };
        return {
            restrict: 'AE',
            replace: true,
            transclude: true,
            scope: {},
            link: linkFunction,
            template: '<div class="panel panel-default">'
                + '<div class="panel-heading">{{header}}'
                + '<span ng-show="headerButton == true" class="pull-right"><a href="{{headerButtonLink}}" class="btn btn-xs btn-primary">{{headerButtonText}}</a></span>'
                + '</div>'
                + '<div class="panel-body">'
                + '</div></div>'
        };
    });

run.$inject = ['$rootScope', '$location', '$cookieStore', '$http', '$route', 'Auth'];
function run($rootScope, $location, $cookieStore, $http, $route, Auth) {
    // Set Site Name
    $rootScope.SiteName = "GameSite"
    $rootScope.Username = null;

    // keep user logged in after page refresh
    if (Auth.CurrentUser) {
        $rootScope.Username = Auth.DisplayName;
        $rootScope.IsAdmin = Auth.IsAdmin;
        $http.defaults.headers.common['Authorization'] = 'Bearer ' + Auth.Token; // jshint ignore:line
    }

    $rootScope.$on('$locationChangeStart', function (event, next, current) {
        // variables
        var requestedPage = $location.path();
        var restrictedPage = $.inArray(requestedPage, ['/login', '/register', '/error']) === -1;

        if ($rootScope.unauthroized) {
            Auth.CurrentUser = null;
            $rootScope.false;
        }

        // hide navbar if not logged in
        if (!Auth.CurrentUser) {
            $(".navbar").hide();
        }
        else {
            $(".navbar").show();
        }

        // redirect to login page if not logged in and trying to access a restricted page
        if (restrictedPage && !Auth.CurrentUser) {
            $rootScope.RequestedPage = $location.path();
            $location.path('/login');
        }
    });

    // Set title on route change
    $rootScope.$on('$routeChangeSuccess', function () {
        $rootScope.PageTitle = $route.current.pageTitle;
    });
}

$(document).ready(function () {

    $("body").click(function (event) {
        // only do this if navigation is visible, otherwise you see jump in navigation while collapse() is called 
        if ($(".navbar-collapse").is(":visible") && $(".navbar-toggle").is(":visible")) {
            $('.navbar-collapse').collapse('toggle');
        }
    });

});