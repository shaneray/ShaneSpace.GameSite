using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameType
    {
        StandardDice = 0,
        StandardCards = 1
    }
}