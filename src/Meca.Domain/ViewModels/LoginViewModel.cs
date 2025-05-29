using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class LoginViewModel
    {
        /// <summary>
        /// Login
        /// </summary>
        [Display(Name = "Login")]
        public string Login { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        /// <example>contato@mecabr.com</example>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        /// <summary>
        /// Senha
        /// </summary>
        /// <example>123123</example>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Senha")]

        public string Password { get; set; }
        /// <summary>
        /// Token de renovação
        /// </summary>
        [Display(Name = "Token de renovação")]

        public string RefreshToken { get; set; }
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