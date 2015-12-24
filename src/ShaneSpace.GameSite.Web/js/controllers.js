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

gameSiteAppControllers.controller('GameController', ['$scope', '$routeParams', 'GameService', 'Config', 'backendHubProxy', '$timeout', '$rootScope', 'Auth', 'snapRemote',
  function ($scope, $routeParams, GameService, Config, backendHubProxy, $timeout, $rootScope, Auth, snapRemote) {

      // Controller init
      var api = new GameService(Config.GameServiceConfig);
      var gameHub = new backendHubProxy(Config.GameServiceUrl, 'gameHub', true);
      var GameChatScrolledToBottom = true;
      var GameChatLoadCount = 0;
      var PrivateChatScrolledToBottom = {};
      var PrivateChatLoadCountHashtable = {};

      // $scope init
      $scope.GameLoading = true;
      $scope.IsPlayer = false;
      $scope.IsHost = false;
      $scope.MyUserId = Auth.CurrentUser.Id;
      $scope.ActiveTab = "PlayArea";
      $scope.JoinGameButtonText = "Join Game";
      $scope.LeaveGameButtonText = "Leave Game";
      $scope.SendButtonText = "Send";
      $scope.GameChatUnreadMessageCount = 0;
      $scope.PrivateChatUnreadMessageCount = {};
      $scope.PrivateChatUnreadMessageCountTotal = 0;
      $scope.MaxDieCount = 15;
      $scope.DieCount = 1;
      $scope.DieArray = [{ DieSideCount: 2 }];
      $scope.DieSides = [2, 4, 6, 8, 10, 12, 20];
      $scope.PlayArea = "";
      $scope.PrivateSendButtonText = [];
      $scope.NewPrivateMessage = [];
      $scope.PrivateChatOpen = [];
      console.log($scope.MyUserId);
      // controller events
      $(resize());
      $(window).resize(function () { resize() });

      function resize() {
          if ($(this).width() > 991) {
              snapRemote.close();
              //snapRemote.disable();
          }
          else {
              snapRemote.enable();
          }
      }
      function CalculateSum(obj) {
          console.log(obj);
          var sum = 0;
          for (var el in obj) {
              if (obj.hasOwnProperty(el)) {
                  sum += parseFloat(obj[el]);
              }
          }
          return sum;
      }
      // disconnect from gameHub when navigating to a different page
      $rootScope.$on("$routeChangeStart", function (event, next, current) {
          gameHub.disconnect();
      });

      // this controls the chat scrolling and unread count
      $scope.$on('onRepeatLast', function (scope, element, attrs) {
          $timeout(function () {
              snapRemote.getSnapper().then(function (snapper) {
                  if (element[0].id == "GameChat") {
                      GameChatLoadCount++;
                      // only scroll the game chat if the chat is visible and already scrolled to the bottom
                      if (GameChatScrolledToBottom === true && ($(window).width() < 991 && snapper.state().state == "right" || $(window).width() > 991) || GameChatLoadCount <= 2) {
                          $scope.GameChatUnreadMessageCount = 0;
                          $scope.ScrollChat(".GameChatContents");
                      }
                      else {
                          // have to devide by 2 because there are technically 2 game chats
                          $scope.GameChatUnreadMessageCount = $scope.GameChatUnreadMessageCount + 1 / 2;
                      }
                      return;
                  }

                  var id = element[0].id + "Container";
                  if (PrivateChatLoadCountHashtable[id] == undefined) { PrivateChatLoadCountHashtable[id] = 0; }
                  if ($scope.PrivateChatUnreadMessageCount[id] == undefined) { $scope.PrivateChatUnreadMessageCount[id] = 0; }
                  PrivateChatLoadCountHashtable[id]++;
                  // only scroll the private chat if the chat is visible and already scrolled to the bottom
                  if (($(window).width() < 991 && snapper.state().state == "left" && $("." + id).is(":visible") && PrivateChatScrolledToBottom[id]
                      || $(window).width() > 991 && $("." + id).is(":visible") && PrivateChatScrolledToBottom[id]) || PrivateChatLoadCountHashtable[id] <= 2) {
                      console.log("scroll " + id + " to bottom");
                      $scope.PrivateChatUnreadMessageCount[id] = 0;
                      $scope.ScrollChat("." + id);
                  }
                  else {
                      console.log("Add to " + id + " unread count");

                      // have to devide by 2 because there are technically 2 game chats
                      $scope.PrivateChatUnreadMessageCount[id] = $scope.PrivateChatUnreadMessageCount[id] + 1 / 2;
                  }
                  $scope.PrivateChatUnreadMessageCountTotal = CalculateSum($scope.PrivateChatUnreadMessageCount);
              });
          });
      });


      // Game hub events
      gameHub.onConnectionSlow(function () { console.log("Realtime connection is slow"); });
      gameHub.onReconnecting(function () { console.log("Realtime connection issues"); });
      gameHub.onReconnected(function () { console.log("Realtime connection issues resolved"); });
      gameHub.onDisconnected(function () { console.log("Realtime connection issues"); });

      gameHub.on('hubMessage', function (data) {
          if (data.Type == "AdminNotification") {
              alert(data.Contents);
          }
          if (data.Type == "GameMessage") {
              $scope.Game.Messages.push(data.Contents);
          }
          if (data.Type == "PrivateMessage") {
              $scope.Game.PrivateMessages.push(data.Contents);
          }
          if (data.Type == "GameAction") {
              $scope.Game.Actions.push(data.Contents);

              if (data.Action === "Joined") {
                  $scope.Game.Players.push(data.Contents.User);
              }
              if (data.Action === "Exited") {
                  angular.forEach($scope.Game.Players, function (value, key) {
                      if (value.Id == data.Contents.User.Id) {
                          $scope.Game.Players.splice(key, 1);
                      }
                  });
              }
              if (data.Action === "StatusChanged") {
                  $scope.Game.Status = data.Contents.AdditionalInfo.Status;
                  if (data.AdditionalInfo.Status == "WaitingForPlayer") {
                      $scope.Game.CurrentPlayer = data.Contents.AdditionalInfo.User;
                  }
                  $scope.StatusChange();
              }
              $scope.PlayerCheck();
          }
          if (data.Type == "OtherPlayerDieChange") {
              $scope.CurrentPlayerDie = data.Contents;
          }
          if (data.Type == "CurrentPlayerRolling") {
              $(".currentPlayerDie").addClass("spinEffect");
          }
      });

      // get game info cllback
      var getGameInfo = function () {
          // should get this info from signalR on connect and reconnect.
          api.Game_GetGameInfo({ "gameId": $routeParams.GameId }).then(function (data) {
              $scope.GameLoading = false;
              $scope.Game = data;
              $scope.StatusChange();
              $scope.PlayerCheck();

              // wait 500ms then set these events
              $timeout(function () {
                  $(".GameChatContents").on("scroll", function () {
                      // keep both game chat instances in sync
                      $(".GameChatContents").scrollTop($(this).scrollTop());

                      // when scrolled to bottom clean the unread message count
                      GameChatScrolledToBottom = this.scrollHeight - this.clientHeight <= this.scrollTop + 1;
                      if (GameChatScrolledToBottom) {
                          $timeout(function () {
                              $scope.GameChatUnreadMessageCount = 0;
                          });
                      }
                  });
                  $(".PrivateChatContents").on("scroll", function (event) {
                      // keep both private chat instances in sync
                      var id = event.currentTarget.id;
                      var scrollTop = $(event.currentTarget).scrollTop();
                      $("." + id).scrollTop(scrollTop);
                      console.log(id + "is scrolling...");

                      // when scrolled to bottom clean the unread message count
                      PrivateChatScrolledToBottom[id] = this.scrollHeight - this.clientHeight <= this.scrollTop + 1;
                      if (PrivateChatScrolledToBottom[id]) {
                          $timeout(function () {
                              console.log(id + " scrolled to bottom");
                              $scope.PrivateChatUnreadMessageCount[id] = 0;
                              $scope.PrivateChatUnreadMessageCountTotal = CalculateSum($scope.PrivateChatUnreadMessageCount);

                              console.log(id + " unread count: " + $scope.PrivateChatUnreadMessageCount[id]);
                          });
                      }

                      // update total
                  });
              }, 500);
          }, function (error) {
              console.error("Error: ", error);
          });
      }

      gameHub.connect("gameId=" + $routeParams.GameId, getGameInfo);

      // Controller Methods
      $scope.PrivateChatOpened = function (id) {
          if ($scope.PrivateChatOpen[id]) {
              $scope.PrivateChatOpen[id] = true;
          }
      }

      $scope.ScrollChat = function (selector) {
          console.log("Scrolling " + selector);
          $(selector).each(function (index) {
              this.scrollTop = this.scrollHeight - this.clientHeight;
          });
      }

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

      // invoke hub methods
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

      $scope.StartGame = function () {
          gameHub.invoke("startGame", null, function (data) {
          });
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

gameSiteAppControllers.controller('FeedbackController', ['$scope', 'GameService', 'Config', '$timeout',
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

gameSiteAppControllers.controller('ErrorController', ['$scope', 'Auth', '$window', '$location',
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