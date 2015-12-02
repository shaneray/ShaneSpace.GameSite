using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Actions
    {
        Created = 0,
        Joined = 1,
        Moved = 2,
        Chat = 3,
        Exited = 4,
        Watching = 5,
        StatusChanged = 6,
        StartedGame = 7
    }
}
