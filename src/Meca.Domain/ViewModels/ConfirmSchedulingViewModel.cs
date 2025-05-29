using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;

namespace Meca.Domain.ViewModels
{
    public class ConfirmSchedulingViewModel
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
        public ConfirmStatus ConfirmSchedulingStatus { get; set; }
    }
}