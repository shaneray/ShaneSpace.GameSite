angular.module("ErrorService", [])
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('errorInterceptor');
        
    }])
    .factory('errorInterceptor', ['$q','$rootScope','$location','$cookieStore',
        function ($q, $rootScope, $location, $cookieStore) {
            $rootScope.ApplicationException = null;
            $rootScope.BadRequest = null;
            return {
                responseError: function (response) {
                    if (response.status == 401) {
                        $rootScope.IsAdmin = false;
                        $rootScope.Username = null;
                        $rootScope.unauthroized = true;
                        $cookieStore.remove('Auth');
                        var requestedPage = $location.path();
                        $location.path(requestedPage);
                        return $q.reject(response);
                    }
                    $rootScope.ApplicationException = response.data;
                    $location.path("/error");
                    return $q.reject(response);
                }
            };
        }]);
