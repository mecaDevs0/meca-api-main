using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels.Admin
{
    public class LoginAdminViewModel
    {
        /// <summary>
        /// Senha
        /// </summary>
        [Display(Name = "Senha")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]

        public string Password { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]

        public string Email { get; set; }
        /// <summary>
        /// Token de renovação
        /// </summary>
        [Display(Name = "Token de renovação")]
        public string RefreshToken { get; set; }
    }
}