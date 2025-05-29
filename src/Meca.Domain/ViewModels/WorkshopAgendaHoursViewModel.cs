using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class WorkshopAgendaHoursViewModel
    {
        /// <summary>
        /// Disponível?
        /// </summary>
        [Display(Name = "Disponível?")]
        public bool Available { get; set; }
        /// <summary>
        /// Hora
        /// </summary>
        [Display(Name = "Hora")]
        public string Hour { get; set; }
        /// <summary>
        /// Usuário
        /// </summary>
        [Display(Name = "Usuário")]
        public ProfileAuxViewModel Profile { get; set; }
        /// <summary>
        /// Veículo
        /// </summary>
        [Display(Name = "Veículo")]
        public VehicleAuxViewModel Vehicle { get; set; }
    }
}