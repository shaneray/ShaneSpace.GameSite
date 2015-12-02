/*jshint -W069 */
/*global angular:false */
/*test */
angular.module('GameProxy', ['AuthService'])
    .factory('GameService', ['$q', '$http', '$rootScope', '$cookieStore',
        function($q, $http, $rootScope, $cookieStore) {
            'use strict';

            /**
             * 
             * @class GameService
             * @param {(string|object)} [domainOrOptions] - The project domain or options object. If object, see the object's optional properties.
             * @param {string} [domainOrOptions.domain] - The project domain
             * @param {string} [domainOrOptions.cache] - An angularjs cache implementation
             * @param {object} [domainOrOptions.token] - auth token - object with value property and optional headerOrQueryName and isQuery properties
             * @param {string} [cache] - An angularjs cache implementation
             */
            var GameService = (function() {
                function GameService(options, cache) {
                    var domain = (typeof options === 'object') ? options.domain : options;
                    this.domain = typeof(domain) === 'string' ? domain : '';
                    if (this.domain.length === 0) {
                        throw new Error('Domain parameter must be specified as a string.');
                    }
                    cache = cache || ((typeof options === 'object') ? options.cache : cache);
                    this.cache = cache;
                    var sessionCurrentUser = $cookieStore.get('Auth') || null;
                    if (sessionCurrentUser) {
                        $http.defaults.headers.common['Authorization'] = 'Bearer ' + sessionCurrentUser.token;
                    }

                }

                GameService.prototype.$on = function($scope, path, handler) {
                    var url = domain + path;
                    $scope.$on(url, function() {
                        handler();
                    });
                    return this;
                };

                GameService.prototype.$broadcast = function(path) {
                    var url = domain + path;
                    //cache.remove(url);
                    $rootScope.$broadcast(url);
                    return this;
                };

                GameService.transformRequest = function(obj) {
                    var str = [];
                    for (var p in obj) {
                        var val = obj[p];
                        if (angular.isArray(val)) {
                            val.forEach(function(val) {
                                str.push(encodeURIComponent(p) + "=" + encodeURIComponent(val));
                            });
                        } else {
                            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(val));
                        }
                    }
                    return str.join("&");
                };

                /**
                 * 
                 * @method
                 * @name GameService#Debug_Echo
                 * @param {string} text - 
                 * 
                 */
                GameService.prototype.Debug_Echo = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/debug/echo';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['text'] !== undefined) {
                        queryParameters['text'] = parameters['text'];
                    }

                    if (parameters['text'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: text'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Feedback_GetFeedback
                 * 
                 */
                GameService.prototype.Feedback_GetFeedback = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/feedback';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Feedback_AddFeedback
                 * @param {} body - 
                 * 
                 */
                GameService.prototype.Feedback_AddFeedback = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/feedback';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['body'] !== undefined) {
                        body = parameters['body'];
                    }

                    if (parameters['body'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: body'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'POST',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Game_GameList
                 * @param {boolean} activeOnly - 
                 * 
                 */
                GameService.prototype.Game_GameList = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/games';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['activeOnly'] !== undefined) {
                        queryParameters['activeOnly'] = parameters['activeOnly'];
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Game_CreateNewGame
                 * @param {} gameConfiguration - 
                 * 
                 */
                GameService.prototype.Game_CreateNewGame = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/games';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['gameConfiguration'] !== undefined) {
                        body = parameters['gameConfiguration'];
                    }

                    if (parameters['gameConfiguration'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: gameConfiguration'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'POST',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Game_GetGameInfo
                 * @param {integer} gameId - 
                 * 
                 */
                GameService.prototype.Game_GetGameInfo = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/games/{gameId}';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    path = path.replace('{gameId}', parameters['gameId']);

                    if (parameters['gameId'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: gameId'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#Game_CreateMessage
                 * @param {integer} gameId - 
                 * @param {string} messageContents - 
                 * 
                 */
                GameService.prototype.Game_CreateMessage = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/games/{gameId}/messages';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    path = path.replace('{gameId}', parameters['gameId']);

                    if (parameters['gameId'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: gameId'));
                        return deferred.promise;
                    }

                    if (parameters['messageContents'] !== undefined) {
                        queryParameters['messageContents'] = parameters['messageContents'];
                    }

                    if (parameters['messageContents'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: messageContents'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'POST',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#User_UserList
                 * 
                 */
                GameService.prototype.User_UserList = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/users';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#User_CreateNewUser
                 * @param {} userConfiguration - 
                 * 
                 */
                GameService.prototype.User_CreateNewUser = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/users';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['userConfiguration'] !== undefined) {
                        body = parameters['userConfiguration'];
                    }

                    if (parameters['userConfiguration'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: userConfiguration'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'POST',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#User_AuthenticateUser
                 * @param {string} email - 
                 * @param {string} password - 
                 * 
                 */
                GameService.prototype.User_AuthenticateUser = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/users/authenticate';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters['email'] !== undefined) {
                        queryParameters['email'] = parameters['email'];
                    }

                    if (parameters['email'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: email'));
                        return deferred.promise;
                    }

                    if (parameters['password'] !== undefined) {
                        queryParameters['password'] = parameters['password'];
                    }

                    if (parameters['password'] === undefined) {
                        deferred.reject(new Error('Missing required  parameter: password'));
                        return deferred.promise;
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'POST',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };
                /**
                 * 
                 * @method
                 * @name GameService#User_GetCurrentUserInfo
                 * 
                 */
                GameService.prototype.User_GetCurrentUserInfo = function(parameters) {
                    if (parameters === undefined) {
                        parameters = {};
                    }
                    var deferred = $q.defer();

                    var domain = this.domain;
                    var path = '/users/current';

                    var body;
                    var queryParameters = {};
                    var form = {};
                    var auth = $cookieStore.get("Auth") || null;
                    if (auth) {
                        var headers = {
                            'Authorization': 'Bearer ' + auth.Token
                        };
                    } else {
                        var headers = {};
                    }

                    if (parameters.$queryParameters) {
                        Object.keys(parameters.$queryParameters)
                            .forEach(function(parameterName) {
                                var parameter = parameters.$queryParameters[parameterName];
                                queryParameters[parameterName] = parameter;
                            });
                    }

                    var url = domain + path;
                    var cached = parameters.$cache && parameters.$cache.get(url);
                    if (cached !== undefined && parameters.$refresh !== true) {
                        deferred.resolve(cached);
                        return deferred.promise;
                    }
                    var options = {
                        timeout: parameters.$timeout,
                        method: 'GET',
                        url: url,
                        params: queryParameters,
                        data: body,
                        headers: headers
                    };
                    if (Object.keys(form).length > 0) {
                        options.data = form;
                        options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                        options.transformRequest = GameService.transformRequest;
                    }

                    $http(options)
                        .success(function(data, status, headers, config) {
                            deferred.resolve(data);
                            if (parameters.$cache !== undefined) {
                                parameters.$cache.put(url, data, parameters.$cacheItemOpts ? parameters.$cacheItemOpts : {});
                            }
                        })
                        .error(function(data, status, headers, config) {
                            deferred.reject({
                                status: status,
                                headers: headers,
                                config: config,
                                body: data
                            });
                        });

                    return deferred.promise;
                };

                return GameService;
            })();

            return GameService;
        }
    ]);