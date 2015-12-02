var gameSiteAppControllers = angular.module('gamesiteAppControllers', ['AuthService', 'ConfigService', 'GameProxy', 'SignalRHubService']);

gameSiteAppControllers.controller('RegisterController', ['$scope', 'Auth', '$rootScope', '$location',
  function ($scope, Auth, $rootScope, $location) {
      $scope.registering = false;

      $scope.Register = function () {
          if ($scope.confirmPassword != $scope.password) {
              $scope.RegistrationError = "Passwords do not match.";
          }
          else {
              $scope.registering = true;
              $scope.RegistrationError = "";

              // do this after auth is complete (pass or fail)
              var registrationCallback = function () {
                  if (Auth.CurrentUser) {
                      // if requested page is not defined or is signout send to dashboard
                      // otherwise navigate to the requesed page.
                      if ($rootScope.RequestedPage == undefined || $rootScope.RequestedPage == "/signout") {
                          $location.path("/dashboard");
                      }
                      else {
                          $location.path($rootScope.RequestedPage);
                      }
                  }
                  else {
                      $scope.registering = false;
                      $scope.RegistrationError = Auth.RegistrationError;
                  }
              }
              Auth.Register($scope.displayName, $scope.email, $scope.password, registrationCallback);
          }
      }
  }]);

gameSiteAppControllers.controller('LoginController', ['$scope', 'Auth', '$rootScope', '$location',
  function ($scope, Auth, $rootScope, $location) {
      // init
      $scope.authenticating = false;

      // do this after auth is complete (pass or fail)
      var authCallback = function () {
          if (Auth.CurrentUser) {
              // if requested page is not defined or is signout send to dashboard
              // otherwise navigate to the requesed page.
              if ($rootScope.RequestedPage == undefined || $rootScope.RequestedPage == "/signout") {
                  $location.path("/dashboard");
              }
              else {
                  $location.path($rootScope.RequestedPage);
              }
          }
          else {
              $scope.authenticating = false;
              $scope.LoginError = Auth.LoginError;
          }
      }

      // if already logged in
      if (Auth.CurrentUser) {
          authCallback();
      }

      $scope.login = function () {
          $scope.authenticating = true;
          Auth.Login($scope.email, $scope.pass, authCallback);
      }
  }]);

gameSiteAppControllers.controller('NavBarController', ['$scope', 'Auth', '$rootScope',
  function ($scope, Auth, $rootScope) {
      if (Auth.CurrentUser) {
          $rootScope.Username = Auth.CurrentUser.DisplayName;
          $rootScope.IsAdmin = Auth.CurrentUser.IsAdmin;
      }
      $scope.signout = function () {
          Auth.LogOut();
      }
  }]);

gameSiteAppControllers.controller('MainController', ['$scope',
  function ($scope) {
  }]);

gameSiteAppControllers.controller('DashboardController', ['$scope',
  function ($scope) {
  }]);

gameSiteAppControllers.controller('NewGameController', ['$scope', '$location', 'GameService', 'Config',
  function ($scope, $location, GameService, Config) {
      var api = new GameService(Config.GameServiceConfig);

      $scope.CreateGame = function () {
          var gameConfig = {
              "GameName": $scope.GameName,
              "ProgressionMode": $scope.ProgressionMode,
              "GameType": $scope.GameType,
              "Rules": $scope.Rules,
              "Description": $scope.Description
          };
          var requestParams = { "gameConfiguration": gameConfig }
          api.Game_CreateNewGame(requestParams).then(function (data) {
              $location.path("/games/" + data.GameId);
          }, function (error) {
              console.error("Error: ", error);
          });
      }
  }]);

gameSiteAppControllers.controller('GameListController', ['$scope', 'GameService', 'Config',
  function ($scope, GameService, Config) {
      $scope.LoadingGames = true;
      var api = new GameService(Config.GameServiceConfig);
      api.Game_GameList().then(function (data) {
          $scope.ActiveGames = [];
          $scope.InActiveGames = [];
          $scope.ClosedGames = [];
          angular.forEach(data, function (value, key) {
              if (value.Status != "Closed" && value.Status != "WaitingForPlayers") {
                  $scope.ActiveGames.push(value);
              }
              if (value.Status == "WaitingForPlayers") {
                  $scope.InActiveGames.push(value);
              }
              if (value.Status == "Closed") {
                  $scope.ClosedGames.push(value);
              }
          });
          $scope.LoadingGames = false;
      }, function (error) {
          console.error("Error: ", error);
      });
  }]);

gameSiteAppControllers.controller('GameController', ['$scope', '$routeParams', 'GameService', 'Config', 'backendHubProxy', '$timeout', '$rootScope', 'Auth','snapRemote',
  function ($scope, $routeParams, GameService, Config, backendHubProxy, $timeout, $rootScope, Auth, snapRemote) {

      // Controller init
      var api = new GameService(Config.GameServiceConfig);
      var gameHub = new backendHubProxy(Config.GameServiceUrl, 'gameHub');

      $scope.GameLoading = true;
      $scope.IsPlayer = false;
      $scope.IsHost = false;
      $scope.MyUserId = Auth.CurrentUser.Id;
      $scope.ActiveTab = "PlayArea";
      $scope.JoinGameButtonText = "Join Game";
      $scope.LeaveGameButtonText = "Leave Game";
      $scope.SendButtonText = "Send";
      $scope.isScrolledToBottom = true;
      $scope.unreadMessageCount = 0;
      $scope.MaxDieCount = 15;
      $scope.DieCount = 1;
      $scope.DieArray = [{ DieSideCount: 2 }];
      $scope.DieSides = [2, 4, 6, 8, 10, 12, 20];
      $scope.PlayArea = "";
      $scope.PrivateSendButtonText = [];
      $scope.NewPrivateMessage = [];

      // controller events
      $(resize());
      $(window).resize(function () { resize() });

      function resize() {
          if ($(this).width() > 991) {
              snapRemote.close();
              snapRemote.disable();
          }
          else {
              snapRemote.enable();
          }
      }

      $("#chatContents").on("scroll", function () {
          var chatContents = document.getElementById("chatContents");
          if (chatContents.scrollHeight - chatContents.clientHeight <= chatContents.scrollTop + 1) {
              $timeout(function () {
                  $scope.unreadMessageCount = 0;
              });
          }
      });

      $rootScope.$on("$routeChangeStart", function (event, next, current) {
          gameHub.disconnect();
      });

      $scope.$on('onRepeatLast', function (scope, element, attrs) {
          if ($scope.isScrolledToBottom === true) {
              $scope.unreadMessageCount = 0;
              $scope.ScrollChat();
          }
          else {
              $timeout(function () {
                  $scope.unreadMessageCount = $scope.unreadMessageCount + 1;
              });
          }
      });

      // Game hub events
      gameHub.on('gameMessage', function (data) {
          var chatContents = document.getElementById("chatContents");

          $scope.isScrolledToBottom = chatContents.scrollHeight - chatContents.clientHeight <= chatContents.scrollTop + 1;
          $scope.Game.Messages.push(data);
      });

      gameHub.on('privateMessage', function (data) {
          $scope.Game.PrivateMessages.push(data);
      });

      gameHub.on('gameAction', function (data) {
          $scope.Game.Actions.push(data);

          if (data.Action === "Joined") {
              $scope.Game.Players.push(data.User);
          }
          if (data.Action === "Exited") {
              angular.forEach($scope.Game.Players, function (value, key) {
                  if (value.Id == data.User.Id) {
                      $scope.Game.Players.splice(key, 1);
                  }
              });
          }
          if (data.Action === "StatusChanged") {
              $scope.Game.Status = data.AdditionalInfo.Status;
              if (data.AdditionalInfo.Status == "WaitingForPlayer") {
                  $scope.Game.CurrentPlayer = data.AdditionalInfo.User;
              }
              $scope.StatusChange();
          }
          $scope.PlayerCheck();
      });

      gameHub.on('otherPlayerDieChange', function (data) {
          $scope.CurrentPlayerDie = data;
      });

      gameHub.on('currentPlayerRolling', function (data) {
          $(".currentPlayerDie").addClass("spinEffect");
      });

      // get game info
      var getGameInfo = function () {
          api.Game_GetGameInfo({ "gameId": $routeParams.GameId }).then(function (data) {
              $scope.GameLoading = false;
              $scope.Game = data;
              $scope.StatusChange();
              $scope.PlayerCheck();

              $('[data-toggle="offcanvas"]').click(function () {
                  $('.row-offcanvas').toggleClass('active')
              });
          }, function (error) {
              console.error("Error: ", error);
          });
      }
      gameHub.connect("gameId=" + $routeParams.GameId, getGameInfo);

      // Controller Methods
      $scope.privateMessageFilter = function (recipientId) {
          return function (message) {
              return (message.Composer.Id == $scope.MyUserId && message.Recipient.Id == recipientId) || (message.Composer.Id == recipientId && message.Recipient.Id == $scope.MyUserId);
          }
      };
      
      $scope.StatusChange = function () {
          $scope.PlayArea = $scope.Game.Status;
          if ($scope.Game.Status == "WaitingForHost") {
              $scope.CurrentPlayer = null;
          }
          if ($scope.Game.Status == "WaitingForPlayer" && $scope.Game.CurrentPlayer != null && $scope.Game.CurrentPlayer.Id == Auth.CurrentUser.Id) {
              $scope.DieArray = [{ DieSideCount: 2 }];
              $scope.DieCount = 1;
              $scope.PlayArea = "WaitingForYou";
              $scope.CurrentPlayerDieChange();
          }
          $scope.PlayerCheck();
      }

      $scope.HostChosePlayer = function () {
          gameHub.invoke("hostChosePlayer", $scope.Game.CurrentPlayer, function () {
          });
      }

      $scope.CurrentPlayerDieChange = function () {
          gameHub.invoke("currentPlayerDieChange", $scope.DieArray, function (data) {
          });
      }

      $scope.RollDie = function () {
          gameHub.invoke("currentPlayerRolledDie", $scope.DieArray, function () {
          });
          $scope.PlayArea = "CurrentPlayerRolling";
      }

      $scope.UpdateDieArray = function () {
          var currentCount = $scope.DieArray.length;
          if (currentCount <= $scope.DieCount) {
              for (var i = currentCount + 1; i <= $scope.DieCount; i++) {
                  $scope.DieArray.push({ DieSideCount: 2 });
              }
          }
          else {
              var removeCount = currentCount - $scope.DieCount;
              var index = 0 - removeCount;
              $scope.DieArray.splice(index);
          }
          $scope.CurrentPlayerDieChange();
      }

      $scope.range = function (n) {
          var result = [];
          for (var i = 1; i <= n; i++) {
              result.push(i);
          }
          return result;
      };

      $scope.StartGame = function () {
          gameHub.invoke("startGame", null, function (data) {

          });
      }

      $scope.ScrollChat = function () {
          var chatContents = document.getElementById("chatContents");
          chatContents.scrollTop = chatContents.scrollHeight - chatContents.clientHeight;
      }

      $scope.JoinGame = function () {
          $scope.JoinGameButtonText = "Joining Game...";
          gameHub.invoke("joinGame", null, function (data) {
              $scope.Game.Actions.push(data);
              $scope.Game.Players.push(data.User);
              $scope.IsPlayer = true;
              $scope.JoinGameButtonText = "Join Game";
          });
      }

      $scope.LeaveGame = function () {
          $scope.LeaveGameButtonText = "Leaving Game...";
          gameHub.invoke("leaveGame", null, function (data) {
              $scope.Game.Actions.push(data);
              angular.forEach($scope.Game.Players, function (value, key) {
                  if (value.Id == data.User.Id) {
                      $scope.Game.Players.splice(key, 1);
                  }
              });
              $scope.IsPlayer = false;
              $scope.LeaveGameButtonText = "Leave Game";
          });
      }

      $scope.AddMessage = function () {
          $scope.SendButtonText = "Sending...";
          var request = {
              "gameId": $scope.Game.GameId,
              "messageContents": $scope.NewMessage
          };
          gameHub.invoke("createMessage", request, function (data) {
              $scope.NewMessage = "";
              $scope.SendButtonText = "Send";
              $scope.isScrolledToBottom = true;
              $scope.Game.Messages.push(data);
          });
      }

      $scope.AddPrivateMessage = function (to, message) {
          $scope.PrivateSendButtonText[to] = "Sending...";
          var request = {
              "gameId": $scope.Game.GameId,
              "RecipientId": to,
              "messageContents": message
          };
          gameHub.invoke("createPrivateMessage", request, function (data) {
              $scope.NewPrivateMessage[to] = "";
              $scope.PrivateSendButtonText[to] = "Send";
              $scope.Game.PrivateMessages.push(data);
          });
      }

      $scope.PlayerCheck = function () {
          if ($scope.Game.Host.Id == Auth.CurrentUser.Id) {
              $scope.IsHost = true;
          }
          else {
              $scope.IsHost = false
          }
          if ($scope.Game.Players.length == 0) {
              $scope.IsPlayer = false;
          }
          angular.forEach($scope.Game.Players, function (value, key) {
              if ($scope.IsPlayer == false) {
                  if (value.Id == Auth.CurrentUser.Id) {
                      $scope.IsPlayer = true;
                  }
              }
          });
      }

      $scope.SetActiveTab = function (name) {
          $scope.ActiveTab = name;
      }
  }]);

gameSiteAppControllers.controller('AdminController', ['$scope', 'GameService', 'Config',
  function ($scope, GameService, Config) {
      $scope.LoadingUsers = true;
      $scope.LoadingFeedback = true;
      var api = new GameService(Config.GameServiceConfig);
      api.User_UserList().then(function (data) {
          $scope.ActiveUsers = [];
          $scope.InActiveUsers = [];
          $scope.BannedUsers = [];
          angular.forEach(data, function (value, key) {
              $scope.ActiveUsers.push(value);
          });
          $scope.LoadingUsers = false;
      }, function (error) {
          console.error("Error: ", error);
      });

      api.Feedback_GetFeedback().then(function (data) {
          $scope.Feedback = data;
          $scope.LoadingFeedback = false;
      }, function (error) {
          console.error("Error: ", error);
      });
  }]);

gameSiteAppControllers.controller('FeedbackController', ['$scope','GameService','Config','$timeout',
    function ($scope, GameService, Config, $timeout) {
        $scope.SubmittingFeedback = false;
        $scope.FeedbackRecieved = false;
        var api = new GameService(Config.GameServiceConfig);
        console.log($scope);

        $scope.SubmitFeedback = function () {
            $scope.SubmittingFeedback = true;

            var request = {
                body: {
                    FeedbackType: $scope.FeedbackType,
                    FeedbackText: $scope.FeedbackText
                }
            };
            
            api.Feedback_AddFeedback(request).then(function (data) {
                $scope.FeedbackRecieved = true;
                $scope.SubmittingFeedback = false;
                $scope.FeedbackType = "";
                $scope.FeedbackText = "";
                $scope.form.$setPristine();
                $timeout(function () {
                    $scope.FeedbackRecieved = false;
                }, 3000);
                
            }, function (error) {
                console.error("Error: ", error);
            });
        }
    }
]);

gameSiteAppControllers.controller('ErrorController', ['$scope','Auth','$window','$location',
  function ($scope, Auth, $window, $location) {
      if (Auth.CurrentUser != null) {
          $scope.IsAuthenticated = true;
      }

      if ($scope.IsAuthenticated && Auth.CurrentUser.IsAdmin) {
          $scope.IsAdmin = true;
      }

      $scope.GoBack = function () {
          $window.history.back();
      }

      $scope.GoToDashboard = function () {
          $location.path('/dashboard');
      }
  }]);