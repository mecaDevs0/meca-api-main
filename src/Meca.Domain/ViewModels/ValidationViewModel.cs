using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class ValidationViewModel
    {

        /// <summary>
        /// E-mail
        /// </summary>
        /// <example>contato@mecabr.com</example>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        /// <summary>
        /// Cpf (999.999.999-99)
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cpf { get; set; }
        /// <summary>
        /// Cnpj (99.999.999/9999-99)
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCnpj(ErrorMessage = DefaultMessages.CnpjInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cnpj { get; set; }
        /// <summary>
        /// Login
        /// </summary>
        [Display(Name = "Login")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }

    }
}