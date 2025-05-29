using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Entities.Auxiliaries;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;

using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class AccessLevelViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Nome
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Name { get; set; }
        /// <summary>
        /// Regras
        /// </summary>
        [Display(Name = "Regras")]
        [LimitElements(1, ErrorMessage = DefaultMessages.NeedElements)]
        [IsReadOnly]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ItemMenuRuleViewModel> Rules { get; set; }
        /// <summary>
        /// É Default
        /// </summary>
        [Display(Name = "É Default")]
        public bool IsDefault { get; set; }
    }
}