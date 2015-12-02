'use strict';

angular.module("SignalRHubService", ['AuthService'])
    .factory('backendHubProxy', ['$rootScope', 'Auth',
  function ($rootScope, Auth) {

      function backendFactory(serverUrl, hubName) {
          var url = serverUrl + '/signalr';
          var connection = $.hubConnection(serverUrl);
          var proxy = connection.createHubProxy(hubName);
          // connection.logging = true;
          return {
              connect: function (queryString, callback) {
                  if (queryString !== undefined) {
                      connection.qs = queryString + '&access_token=' + Auth.CurrentUser.Token;
                  }
                  connection.start()
                    .done(function () {
                        console.log('Hub connected, connection ID=' + connection.id);
                        callback();
                    })
                    .fail(function (e) { console.log(e); });
              },
              disconnect: function () {
                  console.log('Hub disconnected');
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
              }
          }
      }

      return backendFactory;
  }]);