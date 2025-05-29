using System.Collections.Generic;

using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels.HereApi
{

    public class HereApiResponseViewModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("routes")]
        public List<RouteViewModel> Routes { get; set; } = new List<RouteViewModel>();
    }
}