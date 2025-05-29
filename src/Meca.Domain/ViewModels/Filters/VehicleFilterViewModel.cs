using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class VehicleFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Pesquisa (por Placa, Fabricante ou Modelo)
        /// </summary>
        [Display(Name = "Pesquisa (por Placa, Fabricante ou Modelo)")]
        public string Search { get; set; }
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
        /// ID do usuário
        /// </summary>
        [Display(Name = "ID do usuário")]
        public string ProfileId { get; set; }
    }
}