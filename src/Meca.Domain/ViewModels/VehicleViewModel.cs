using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels
{
    public class VehicleViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Placa do veículo
        /// </summary>
        [Display(Name = "Placa do veículo")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Plate { get; set; }
        /// <summary>
        /// Fabricante
        /// </summary>
        [Display(Name = "Fabricante")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Manufacturer { get; set; }
        /// <summary>
        /// Modelo
        /// </summary>
        [Display(Name = "Modelo")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Model { get; set; }
        /// <summary>
        /// Quilometragem
        /// </summary>
        [Display(Name = "Quilometragem")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double? Km { get; set; }
        /// <summary>
        /// Cor
        /// </summary>
        [Display(Name = "Cor")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Color { get; set; }
        /// <summary>
        /// Ano
        /// </summary>
        [Display(Name = "Ano")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Year { get; set; }
        /// <summary>
        /// Data da última revisão
        /// </summary>
        [Display(Name = "Data da última revisão")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public long? LastRevisionDate { get; set; }
        /// <summary>
        /// Usuário
        /// </summary>
        [Display(Name = "Usuário")]
        public ProfileAuxViewModel Profile { get; set; }
    }
}