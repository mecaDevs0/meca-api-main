using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class DisputeDisapprovedServiceViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Contestação
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Contestação")]
        public string Dispute { get; set; }
        /// <summary>
        /// Imagens da contestação
        /// </summary>
        [Display(Name = "Imagens da contestação")]
        public List<string> ImagesDispute { get; set; }
    }
}