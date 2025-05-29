using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels
{
    public class TravelViewModel
    {

        [JsonProperty("fuelCost")]
        public double FuelCost { get; set; }

        [JsonProperty("tollCost")]
        public double TollCost { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }


}