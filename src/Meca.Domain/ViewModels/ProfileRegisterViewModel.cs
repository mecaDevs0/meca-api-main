using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class ProfileRegisterViewModel : ProfileViewModel
    {
        /// <summary>
        /// Senha
        /// </summary>
        [Display(Name = "Senha")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsReadOnly]
        public string Password { get; set; }
        /// <summary>
        /// Token de rede social
        /// </summary>
        [Display(Name = "Token de rede social")]
        public string ProviderId { get; set; }
        /// <summary>
        /// Tipo de token social
        /// </summary>
        [Display(Name = "Tipo de token social")]
        public TypeProvider TypeProvider { get; set; }
    }
}