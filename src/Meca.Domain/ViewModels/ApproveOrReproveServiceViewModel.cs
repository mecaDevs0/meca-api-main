using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class ApproveOrReproveServiceViewModel
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
        public ConfirmStatus ConfirmStatus { get; set; }
        /// <summary>
        /// Lista de serviços aprovados
        /// </summary>
        [Display(Name = "Lista de serviços aprovados")]
        public List<BudgetServicesAuxViewModel> BudgetServices { get; set; }
    }
}