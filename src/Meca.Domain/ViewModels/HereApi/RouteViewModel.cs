using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arteris.Domain.ViewModels.HereApi
{
    public class RouteViewModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sections")]
        public List<SectionViewModel> Sections { get; set; }
    }
}