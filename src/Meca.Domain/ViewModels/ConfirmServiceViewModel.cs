using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class ConfirmServiceViewModel
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
        public ConfirmStatus ConfirmServiceStatus { get; set; }
        /// <summary>
        /// Motivo da reprovação
        /// </summary>
        [RequiredIf("ConfirmServiceStatus", ConfirmStatus.Disapprove, ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Motivo da reprovação")]
        public string ReasonDisapproval { get; set; }
        /// <summary>
        /// Imagens (da reprovação)
        /// </summary>
        [Display(Name = "Imagens (da reprovação)")]
        public List<string> ImagesDisapproval { get; set; }
    }
}