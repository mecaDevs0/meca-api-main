using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class ConfirmBudgetViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Status (Aprovado/Reprovado)
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Status (Aprovado/Reprovado)")]
        public ConfirmStatus ConfirmBudgetStatus { get; set; }
        /// <summary>
        /// Lista de serviços
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Lista de serviços")]
        public List<BudgetServicesAuxViewModel> BudgetServices { get; set; }
    }
}