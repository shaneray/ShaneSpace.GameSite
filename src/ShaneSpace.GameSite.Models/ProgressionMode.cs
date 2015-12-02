using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProgressionMode
    {
        RoundRobin = 0,
        HostChoice = 1
    }
}