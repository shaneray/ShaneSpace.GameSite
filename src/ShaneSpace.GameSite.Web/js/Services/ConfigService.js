angular.module("ConfigService", [])
    .factory('Config', [
    function () {
        var Config = {
            GameServiceConfig: { domain: 'http://localhost:62614' },
            GameServiceUrl: "http://localhost:62614"
            //GameServiceConfig: { domain: 'http://shanespace.duckdns.org:2023' },
            //GameServiceUrl: "http://shanespace.duckdns.org:2023"
        };

        return Config;
    }]);