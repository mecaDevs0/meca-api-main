using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels.HereApi
{
    public class SectionViewModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("summary")]
        public SummaryViewModel Summary { get; set; }

        [JsonProperty("polyline")]
        public string Polyline { get; set; }
    }
}