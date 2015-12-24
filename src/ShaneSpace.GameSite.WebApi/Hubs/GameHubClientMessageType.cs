using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public static class GameHubClientMessageType
    {
        public static string UNKNOWN = "UNKNOWN";
        public static string GameMessage = "GameMessage";
        public static string PrivateMessage = "PrivateMessage";
        public static string GameAction = "GameAction";
        public static string OtherPlayerDieChange = "OtherPlayerDieChange";
        public static string CurrentPlayerRolling = "CurrentPlayerRolling";
        public static string AdminNotification = "AdminNotification";
    }
}