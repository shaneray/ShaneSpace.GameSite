'use strict';

angular.module("SignalRHubService", ['AuthService'])
    .factory('backendHubProxy', ['$rootScope', 'Auth', '$cookieStore', '$location',
  function ($rootScope, Auth, $cookieStore, $location) {

      function backendFactory(serverUrl, hubName, debug) {
          var url = serverUrl + '/signalr';
          var connection = $.hubConnection(serverUrl);
          var proxy = connection.createHubProxy(hubName);
          if (debug != undefined && debug != null) {
              connection.logging = debug;
          }

          var connectionSlowCallback = function () { };
          var reconnectingCallback = function () { };
          var reconnectedCallback = function () { };
          var disconnectedCallback = function () { };

          connection.connectionSlow(function () {
              if (connection.logging === true) { console.log("SignalR connection SLOW."); }
              connectionSlowCallback();
          });
          connection.reconnecting(function () {
              if (connection.logging === true) { console.log("SignalR connection RECONNECTING."); }
              reconnectingCallback();
          });
          connection.reconnected(function () {
              if (connection.logging === true) { console.log("SignalR connection RECONNECTED."); }
              reconnectedCallback();
          });
          connection.disconnected(function () {
              if (connection.logging === true) { console.log("SignalR connection DISCONNECTED."); }
              disconnectedCallback();
          });
          connection.error(function (error) {
              if (error.source.status == 401) {
                  $rootScope.IsAdmin = false;
                  $rootScope.Username = null;
                  $rootScope.unauthroized = true;
                  $cookieStore.remove('Auth');
                  var requestedPage = $location.path();
                  $location.path(requestedPage);
              }
          });

          return {
              connect: function (queryString, callback) {
                  if (queryString !== undefined) {
                      connection.qs = queryString + '&access_token=' + Auth.CurrentUser.Token;
                  }
                  connection.start()
                    .done(function () {
                        if (connection.logging == true) { console.log('Hub connected, connection ID=' + connection.id); }
                        callback();
                    })
                    .fail(function (e, e2) { console.log(e2); });
              },
              disconnect: function () {
                  if (connection.logging == true) { console.log('Hub disconnected'); }
                  connection.stop();
              },
              on: function (eventName, callback) {
                  proxy.on(eventName, function (result) {
                      $rootScope.$apply(function () {
                          if (callback) {
                              callback(result);
                          }
                      });
                  });
              },
              invoke: function (methodName, parameters, callback) {
                  proxy.invoke(methodName, parameters)
                  .done(function (result) {
                      $rootScope.$apply(function () {
                          if (callback) {
                              callback(result);
                          }
                      });
                  })
                  .fail(function (e) { console.log(e); });
              },
              onDisconnected: function (callback) {
                  disconnectedCallback = callback;
              },
              onReconnecting: function (callback) {
                  reconnectingCallback = callback;
              },
              onReconnected: function (callback) {
                  reconnectedCallback = callback;
              },
              onConnectionSlow: function (callback) {
                  connectionSlowCallback = callback;
              }
          }
      }

      return backendFactory;
  }]);