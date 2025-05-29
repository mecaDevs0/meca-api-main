using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Models;


namespace Meca.Domain.ViewModels
{
    public class WorkshopViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Nome do responsável
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome do responsável")]
        public string FullName { get; set; }
        /// <summary>
        /// Data de nascimento do Responsável (DD/MM/AAAA)
        /// </summary>
        [Display(Name = "Data de nascimento do Responsável")]
        [DateOfBirth(ErrorMessage = DefaultMessages.DateOfBirthInvalid)]
        public string BirthDate { get; set; }
        /// <summary>
        /// Nome da Empresa
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome da Empresa")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Documento com foto
        /// </summary>
        [Display(Name = "Documento com foto")]
        [JsonConverter(typeof(PathImage))]
        public string FileDocument { get; set; }
        /// <summary>
        /// Cpf do Responsável
        /// </summary>
        [Display(Name = "Cpf do Responsável")]
        [IsValidCpf(ErrorMessage = DefaultMessages.CpfInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cpf { get; set; }
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
        /// Logo (Nome retornado pela api de upload ex (3142315321.png))
        /// </summary>
        /// <exemple>323141235125.png</example>
        [Display(Name = "Logo")]
        [JsonConverter(typeof(PathImage))]
        public string Photo { get; set; }
        /// <summary>
        /// Cartão MEI (Nome retornado pela api de upload ex (3142315321.png))
        /// </summary>
        /// <exemple>323141235125.png</example>
        [Display(Name = "Cartão MEI")]
        [JsonConverter(typeof(PathImage))]
        [RequiredIf(nameof(Cnpj), null, false, ErrorMessage = DefaultMessages.FieldRequired)]
        public string MeiCard { get; set; }
        /// <summary>
        /// Cnpj (99.999.999/9999-99)
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsValidCnpj(ErrorMessage = DefaultMessages.CnpjInvalid)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Cnpj { get; set; }
        /// <summary>
        /// Telefone do responsável (99) 9999.9999 || (99) 99999-9999
        /// </summary>
        /// <example>(11) 2020-2020</example>
        [Display(Name = "Telefone do responsável")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [JsonConverter(typeof(OnlyNumber))]
        public string Phone { get; set; }
        /// <summary>
        /// Motivo de bloqueio
        /// </summary>
        [Display(Name = "Motivo de bloqueio")]
        public string Reason { get; set; }
        /// <summary>
        /// Média de avaliações
        /// </summary>
        [Display(Name = "Média de avaliações")]
        public double? Rating { get; set; }
        /// <summary>
        /// Distância em KM
        /// </summary>
        [Display(Name = "Distância em KM")]
        [IsReadOnly]
        public double? Distance { get; set; }
        /// <summary>
        /// Cep
        /// </summary>
        [Display(Name = "Cep")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string ZipCode { get; set; }
        /// <summary>
        /// Rua
        /// </summary>
        [Display(Name = "Rua")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Número
        /// </summary>
        [Display(Name = "Número")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Number { get; set; }
        /// <summary>
        /// Nome da Cidade
        /// </summary>
        [Display(Name = "Nome da Cidade")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string CityName { get; set; }
        /// <summary>
        /// Identificador da cidade
        /// </summary>
        [Display(Name = "Identificador da cidade")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string CityId { get; set; }
        /// <summary>
        /// Nome do Estado
        /// </summary>
        [Display(Name = "Nome do Estado")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string StateName { get; set; }
        /// <summary>
        /// Uf do Estado
        /// </summary>
        [Display(Name = "Uf do Estado")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string StateUf { get; set; }
        /// <summary>
        /// Identificador do estado
        /// </summary>
        [Display(Name = "Identificador do estado")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string StateId { get; set; }
        /// <summary>
        /// Bairro
        /// </summary>
        [Display(Name = "Bairro")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Neighborhood { get; set; }
        /// <summary>
        /// Complemento
        /// </summary>
        [Display(Name = "Complemento")]
        public string Complement { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [Display(Name = "Latitude")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [Display(Name = "Longitude")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double Longitude { get; set; }
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
        /// Status
        /// </summary>
        [Display(Name = "Status")]
        public WorkshopStatus Status { get; set; }
        /// <summary>
        /// Dados bancários válido?
        /// </summary>
        [Display(Name = "Dados bancários válido?")]
        public bool DataBankValid { get; set; }
        /// <summary>
        /// Agenda configurada?
        /// </summary>
        [Display(Name = "Agenda configurada?")]
        public bool WorkshopAgendaValid { get; set; }
        /// <summary>
        /// Serviços configurados?
        /// </summary>
        [Display(Name = "Serviços configurados?")]
        public bool WorkshopServicesValid { get; set; }
        /// <summary>
        /// Pendências   GATEWAY
        /// </summary>
        [Display(Name = "Pendências")]
        public List<StripeAccountRequirement> Requirements { get; set; } = [];
    }
}