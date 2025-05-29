
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class InfoAddressViewModel
    {
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
        public string Gia { get; set; }
        /// <summary>
        /// Código do Ibge
        /// </summary>
        /// <value></value>
        public string Ibge { get; set; }
    }
}