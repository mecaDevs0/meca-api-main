using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels
{
    public class VehicleInfoViewModel
    {
        /// <summary>
        /// Placa do veículo
        /// </summary>
        [Display(Name = "Placa do veículo")]
        public string Plate { get; set; }
        /// <summary>
        /// Fabricante
        /// </summary>
        [Display(Name = "Fabricante")]
        public string Manufacturer { get; set; }
        /// <summary>
        /// Modelo
        /// </summary>
        [Display(Name = "Modelo")]
        public string Model { get; set; }
        /// <summary>
        /// Cor
        /// </summary>
        [Display(Name = "Cor")]
        public string Color { get; set; }
        /// <summary>
        /// Ano de fabricação
        /// </summary>
        [Display(Name = "Ano de fabricação")]
        public string Year { get; set; }
    }
}