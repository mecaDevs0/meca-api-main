using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class VehicleAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        public string Id { get; set; }
        /// <summary>
        /// Placa do carro
        /// </summary>
        [Display(Name = "Placa do carro")]
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
    }
}