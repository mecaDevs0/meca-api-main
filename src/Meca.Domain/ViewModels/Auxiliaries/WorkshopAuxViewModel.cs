using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class WorkshopAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        public string Id { get; set; }
        /// <summary>
        /// Nome completo
        /// </summary>
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }
        /// <summary>
        /// Nome da oficina
        /// </summary>
        [Display(Name = "Nome da oficina")]
        public string CompanyName { get; set; }
        /// <summary>
        /// Telefone do responsável
        /// </summary>
        [Display(Name = "Telefone do responsável")]
        public string Phone { get; set; }
        /// <summary>
        /// E-mail do responsável
        /// </summary>
        /// <example>contato@mecabr.com</example>
        [Display(Name = "E-mail do responsável")]
        public string Email { get; set; }
        /// <summary>
        /// Cnpj (99.999.999/9999-99)
        /// </summary>
        [Display(Name = "Cnpj")]
        public string Cnpj { get; set; }

        /*ADDRESS INFO*/
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
        /// Latitude
        /// </summary>
        [Display(Name = "Latitude")]
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }
        /// <summary>
        /// Descrição/Motivo
        /// </summary>
        [Display(Name = "Descrição/Motivo")]
        public string Reason { get; set; }
    }
}