angular.module("AuthService", ['ConfigService']).factory('Auth', ['$rootScope', '$location', '$cookieStore', 'Config', 'GameService',
    function ($rootScope, $location, $cookieStore, Config, GameService) {
        // init
        var api = new GameService(Config.GameServiceConfig);

        var Auth = {
            CurrentUser: null,
            LoginError: "",
            RegistrationError: ""
        };

        // if user is saved in session load it up.
        var sessionCurrentUser = $cookieStore.get('Auth') || null;
        if (sessionCurrentUser) {
            Auth.CurrentUser = sessionCurrentUser;
        }

        // username, password, and function to call when complete(pass or fail).
        Auth.Login = function (Email, Pass, AuthCallback) {
            var data = {
                email: Email,
                password: Pass
            };

            api.User_AuthenticateUser(data)
                .then(function (response) {
                    // login method adds user token to session storage
                    // and adds username to root scope
                    Auth.CurrentUser = {};
                    Auth.CurrentUser.Token = response;
                    $cookieStore.put('Auth', Auth.CurrentUser);

                    // get claims
                    api.User_GetCurrentUserInfo()
                        .then(function (response) {
                            Auth.CurrentUser.Id = response.Id;
                            Auth.CurrentUser.Email = response.Email;
                            Auth.CurrentUser.DisplayName = response.DisplayName;
                            if (response.Roles.indexOf("Admin") != -1) {
                                Auth.CurrentUser.IsAdmin = true;
                            }
                            else {
                                Auth.CurrentUser.IsAdmin = false;
                            }
                            $cookieStore.put('Auth', Auth.CurrentUser);
                            $rootScope.Username = Auth.CurrentUser.DisplayName;
                            $rootScope.IsAdmin = Auth.CurrentUser.IsAdmin;

                            AuthCallback();
                        }, function (error) {
                            // called asynchronously if an error occurs
                            // or server returns response with an error status.
                            Auth.LoginError = "Unable to retrieve user information. Please try again.";
                            Auth.CurrentUser = null;
                            AuthCallback();
                        });
                }, function (error) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
                    Auth.LoginError = "Invalid Credentials. Please try again.";
                    Auth.CurrentUser = null;
                    AuthCallback();
                });
        }

        Auth.Register = function (DisplayName, Email, Pass, RegistrationCallback) {
            var data = {
                DisplayName: DisplayName,
                Email: Email,
                Password: Pass
            };

            api.User_CreateNewUser({ userConfiguration: data })
                .then(function (response) {
                    Auth.RegistrationError = '';
                    Auth.Login(Email, Pass, RegistrationCallback);
                }, function (response) {
                    Auth.RegistrationError = response.body.ExceptionMessage;
                    RegistrationCallback();
                });
        }

        Auth.LogOut = function () {
            Auth.CurrentUser = null;
            $rootScope.IsAdmin = false;
            $rootScope.Username = null;
            $cookieStore.remove('Auth');
            $location.path("/signout");
        }

        return Auth;
    }]);