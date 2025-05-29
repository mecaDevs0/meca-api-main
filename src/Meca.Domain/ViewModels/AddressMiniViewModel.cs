using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class AddressMiniViewModel : BaseViewModel
    {
        /// <summary>
        /// Endereço formatado
        /// </summary>
        [Display(Name = "Endereço formatado")]
        public string FormatedAddress { get; set; }
        /// <summary>
        /// Nome do Estado
        /// </summary>
        [Display(Name = "Nome do Estado")]
        public string State { get; set; }
        /// <summary>
        /// Nome da cidade
        /// </summary>
        [Display(Name = "Nome da cidade")]
        public string City { get; set; }
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
    }
}