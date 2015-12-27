angular.module("ConfigService", [])
    .factory('Config', [
    function () {
        var configUrl = 'http://localhost:62614'

        var configOverride = '$(GameServiceUrl)';
        if (configOverride.indexOf("GameServiceUrl") === -1) {
            configUrl = configOverride;
        }

        var Config = {
            GameServiceConfig: { domain: configUrl },
            GameServiceUrl: configUrl
        };

        return Config;
    }]);