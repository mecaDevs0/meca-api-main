using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class SchedulingAvailableFilterViewModel
    {
        /// <summary>
        /// Data
        /// </summary>
        [Display(Name = "Data")]
        public long Date { get; set; }
        /// <summary>
        /// Lista de serviços
        /// </summary>
        [Display(Name = "Lista de serviços")]
        public List<WorkshopServicesAuxViewModel> Services { get; set; }
        /// <summary>
        /// ID da Oficina
        /// </summary>
        [Display(Name = "ID da Oficina")]
        public string WorkshopId { get; set; }
    }
}