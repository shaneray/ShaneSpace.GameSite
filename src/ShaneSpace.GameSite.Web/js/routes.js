config.$inject = ['$routeProvider', '$locationProvider'];
function config($routeProvider, $locationProvider) {
    $routeProvider.
        when('/', {
            templateUrl: 'partials/Dashboard.html',
            controller: 'DashboardController',
            pageTitle: 'Dashboard'
        }).
        when('/signout', {
            templateUrl: 'partials/Dashboard.html',
            controller: 'DashboardController',
            pageTitle: 'Dashboard'
        }).
        when('/register', {
            templateUrl: 'partials/Register.html',
            controller: 'RegisterController',
            pageTitle: 'Login'
        }).
        when('/login', {
            templateUrl: 'partials/Login.html',
            controller: 'LoginController',
            pageTitle: 'Login'
        }).
        when('/dashboard', {
            templateUrl: 'partials/Dashboard.html',
            controller: 'DashboardController',
            pageTitle: 'Dashboard'
        }).
        when('/games', {
            templateUrl: 'partials/games/List.html',
            controller: 'GameListController',
            pageTitle: 'Games'
        }).
          when('/games/new', {
              templateUrl: 'partials/games/New.html',
              controller: 'NewGameController',
              pageTitle: 'NewGame'
          }).
        when('/games/:GameId', {
            templateUrl: 'partials/games/Game.html',
            controller: 'GameController',
            pageTitle: 'Game'
        }).
        when('/admin', {
            templateUrl: 'partials/Admin.html',
            controller: 'AdminController',
            pageTitle: 'Admin'
        }).
        when('/error', {
            templateUrl: 'partials/Error.html',
            controller: 'ErrorController',
            pageTitle: 'Error'
        }).
        when('/feedback', {
            templateUrl: 'partials/Feedback.html',
            controller: 'FeedbackController',
            pageTitle: 'Feedback'
        }).
        otherwise({
            redirectTo: '/dashboard'
        });
}