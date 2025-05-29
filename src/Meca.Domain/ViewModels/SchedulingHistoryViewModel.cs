using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels
{
    public class SchedulingHistoryViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Titulo do status
        /// </summary>
        [Display(Name = "Titulo do status")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public SchedulingStatusTitle StatusTitle { get; set; }
        /// <summary>
        /// Status do agendamento
        /// </summary>
        [Display(Name = "Status do agendamento")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public SchedulingStatus Status { get; set; }
        /// <summary>
        /// Descrição do status
        /// </summary>
        [Display(Name = "Descrição do status")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Description { get; set; }
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string SchedulingId { get; set; }
    }
}