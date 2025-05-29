using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class BlockViewModel
    {
        /// <summary>
        /// Bloquear
        /// </summary>
        [Display(Name = "Bloquear")]
        public bool Block { get; set; }
        /// <summary>
        /// Motivo
        /// </summary>
        [Display(Name = "Motivo")]
        public string Reason { get; set; }
        /// <summary>
        /// Identificador 
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [MinLength(24, ErrorMessage = DefaultMessages.InvalidIdentifier)]
        public string TargetId { get; set; }
    }
}