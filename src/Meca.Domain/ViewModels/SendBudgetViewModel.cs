using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Entities.Auxiliaries;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class SendBudgetViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Serviços
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Serviços")]
        public List<BudgetServicesAuxViewModel> BudgetServices { get; set; }
        /// <summary>
        /// Valor do diagnóstico 
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Valor do diagnóstico ")]
        public double DiagnosticValue { get; set; }
        /// <summary>
        /// Imagens 
        /// </summary>
        [Display(Name = "Imagens ")]
        [JsonConverter(typeof(PathImage))]
        public List<string> BudgetImages { get; set; }
        /// <summary>
        /// Data estimada para conclusão 
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Data estimada para conclusão ")]
        public long EstimatedTimeForCompletion { get; set; }
    }
}