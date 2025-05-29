using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class DataBankViewModel : BaseViewModel
    {
        /// <summary>
        /// Informou dados bancários
        /// </summary>
        [Display(Name = "Informou dados bancários")]
        [IsReadOnly]
        public bool HasDataBank { get; set; }
        /// <summary>
        /// Nome do Responsável
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome do Responsável")]
        public string AccountableName { get; set; }
        /// <summary>
        /// Cpf do Responsável
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Cpf do Responsável")]
        [JsonConverter(typeof(OnlyNumber))]
        public string AccountableCpf { get; set; }
        /// <summary>
        /// Conta
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Conta")]
        public string BankAccount { get; set; }
        /// <summary>
        /// Agência
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Agência")]

        public string BankAgency { get; set; }
        /// <summary>
        /// Código do banco
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Banco")]
        public string Bank { get; set; }
        /// <summary>
        /// Nome do banco
        /// </summary>
        [Display(Name = "Nome do banco")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string BankName { get; set; }
        /// <summary>
        /// Tipo de conta
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo de conta")]
        public TypeAccount TypeAccount { get; set; }
        /// <summary>
        /// Tipo de pessoa
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo de pessoa")]
        public TypePersonBank PersonType { get; set; }
        /// <summary>
        /// Cnpj
        /// </summary>
        [Display(Name = "Cnpj")]
        [JsonConverter(typeof(OnlyNumber))]
        [IsValidCnpj(ErrorMessage = DefaultMessages.CnpjInvalid)]
        [RequiredIf(nameof(PersonType), TypePersonBank.LegalPerson, ErrorMessage = DefaultMessages.FieldRequired)]
        public string BankCnpj { get; set; }
        /// <summary>
        /// Status da conta
        /// </summary>
        [Display(Name = "Status da conta")]
        [IsReadOnly]
        public DataBankStatus DataBankStatus { get; set; }
    }
}