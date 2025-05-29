using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Senha atual
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Senha atual")]
        public string CurrentPassword { get; set; }
        /// <summary>
        /// Nova senha
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }
    }
}