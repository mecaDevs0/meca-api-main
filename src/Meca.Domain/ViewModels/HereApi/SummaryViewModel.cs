using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels.HereApi
{
    public class SummaryViewModel
    {

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("baseDuration")]
        public int BaseDuration { get; set; }
    }
}