using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class SuggestNewTimeViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Horário sugerido
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Horário sugerido")]
        public long Date { get; set; }
    }
}