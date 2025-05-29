using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class SiteMecaFormViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Nome completo
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }
        /// <summary>
        /// Telefone (99) 9999.9999 || (99) 99999-9999
        /// </summary>
        /// <example>(11) 2020-2020</example>
        [Display(Name = "Telefone")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Phone { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        /// <example>contato@mecabr.com</example>
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        /// <summary>
        /// Descrição
        /// </summary>
        [Display(Name = "Descrição")]
        [IsValidCpf(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Description { get; set; }
    }
}