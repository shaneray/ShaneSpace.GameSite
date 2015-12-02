using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameStatus
    {
        WaitingForPlayers = 0,
        Active = 1,
        Closed = 3,
        WaitingForHost = 4,
        WaitingForPlayer = 5
    }
}
