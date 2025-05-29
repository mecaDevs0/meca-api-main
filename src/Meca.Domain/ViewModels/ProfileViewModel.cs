using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class ProfileViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Nome completo
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }
        /// <summary>
        /// Login
        /// </summary>
        [Display(Name = "Login")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Login { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        /// <example>contato@mecabr.com</example>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        /// <summary>
        /// Foto (Nome retornado pela api de upload ex (3142315321.png))
        /// </summary>
        /// <exemple>323141235125.png</example>
        [Display(Name = "Foto")]
        [JsonConverter(typeof(PathImage))]
        public string Photo { get; set; }
        /// <summary>
        /// Cpf (999.999.999-99)
        /// </summary>
        /// <example>364.818.768-69</example>
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cpf { get; set; }
        /// <summary>
        /// Telefone (99) 9999.9999 || (99) 99999-9999
        /// </summary>
        /// <example>(11) 2020-2020</example>
        [Display(Name = "Telefone")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Phone { get; set; }
        /// <summary>
        /// Cep
        /// </summary>
        [Display(Name = "Cep")]
        public string ZipCode { get; set; }
        /// <summary>
        /// Rua
        /// </summary>
        [Display(Name = "Rua")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Número
        /// </summary>
        [Display(Name = "Número")]
        public string Number { get; set; }
        /// <summary>
        /// Nome da Cidade
        /// </summary>
        [Display(Name = "Nome da Cidade")]
        public string CityName { get; set; }
        /// <summary>
        /// Identificador da cidade
        /// </summary>
        [Display(Name = "Identificador da cidade")]
        public string CityId { get; set; }
        /// <summary>
        /// Nome do Estado
        /// </summary>
        [Display(Name = "Nome do Estado")]
        public string StateName { get; set; }
        /// <summary>
        /// Uf do Estado
        /// </summary>
        [Display(Name = "Uf do Estado")]
        public string StateUf { get; set; }
        /// <summary>
        /// Identificador do estado
        /// </summary>
        [Display(Name = "Identificador do estado")]
        public string StateId { get; set; }
        /// <summary>
        /// Bairro
        /// </summary>
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; }
        /// <summary>
        /// Complemento
        /// </summary>
        [Display(Name = "Complemento")]
        public string Complement { get; set; }
        /// <summary>
        /// Bloqueado
        /// </summary>
        [Display(Name = "Bloqueado")]
        public bool Blocked { get; set; }
        /// <summary>
        /// Total de notifações não lidas
        /// </summary>
        [Display(Name = "Total de notifações não lidas")]
        [IsReadOnly]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalNotificationNoRead { get; set; }

        /// <summary>
        /// Identificador externo
        /// </summary>
        [Display(Name = "Identificador externo")]
        [IsReadOnly]
        public string ExternalId { get; set; }

    }
}