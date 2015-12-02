using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaneSpace.GameSite.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FeedbackTypes
    {
        BugReport = 1,
        FeatureRequest = 2,
        Other = 3
    }
}