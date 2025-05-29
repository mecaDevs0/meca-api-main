using System.Collections.Generic;
using Arteris.Domain.ViewModels.HereApi;
using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels
{


    public class CalculateKmResponseViewModel
    {

        [JsonProperty("travel")]
        public TravelViewModel Travel { get; set; }

        [JsonProperty("polyline")]
        public string Polyline { get; set; }

        [JsonProperty("points")]
        public List<LatLngZ> Points { get; set; } = new List<LatLngZ>();
    }


}