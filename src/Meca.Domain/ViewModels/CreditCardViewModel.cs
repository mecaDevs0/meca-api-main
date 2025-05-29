using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class CreditCardViewModel : BaseViewModel
    {
        /// <summary>
        /// Nome no cartão
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome")]
        public string Name { get; set; }
        /// <summary>
        /// Numero do cartão
        /// </summary>
        /// <example>4111 1111 1111 1111</example>
        [MinLength(14, ErrorMessage = DefaultMessages.Minlength)]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número do cartão")]
        public string Number { get; set; }
        /// <summary>
        /// Mês de vencimento (1 a 12)
        /// </summary>
        [Range(1, 12, ErrorMessage = DefaultMessages.Range)]
        [Display(Name = "Mês de vencimento")]
        public int ExpMonth { get; set; }
        /// <summary>
        /// Código de segurança
        /// </summary>
        [MinLength(3, ErrorMessage = DefaultMessages.Minlength)]
        [MaxLength(4, ErrorMessage = DefaultMessages.Maxlength)]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [JsonConverter(typeof(OnlyNumber))]
        [Display(Name = "Código de segurança")]
        public string Cvv { get; set; }
        /// <summary>
        /// Ano de vencimento (AAAA)
        /// </summary>
        [Display(Name = "Ano de vencimento")]
        public int ExpYear { get; set; }
        /// <summary>
        /// Bandeira (Link imagem)
        /// </summary>
        [Display(Name = "Bandeira")]
        public string Flag { get; set; }
        /// <summary>
        /// Bandeira (nome)
        /// </summary>
        [Display(Name = "Bandeira ")]
        public string Brand { get; set; }

    }
}