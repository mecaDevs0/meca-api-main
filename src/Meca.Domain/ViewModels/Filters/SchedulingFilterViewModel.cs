using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels.Filters
{
    public class SchedulingFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Início do filtro
        /// </summary>
        [Display(Name = "Início do filtro")]
        public long? StartDate { get; set; }
        /// <summary>
        /// Fim do filtro
        /// </summary>
        [Display(Name = "Fim do filtro")]
        public long? EndDate { get; set; }
        /// <summary>
        /// ID da oficina
        /// </summary>
        [Display(Name = "ID da oficina")]
        public string WorkshopId { get; set; }
        /// <summary>
        /// ID do usuário
        /// </summary>
        [Display(Name = "ID do usuário")]
        public string? ProfileId { get; set; }
        /// <summary>
        /// Status do agendamento
        /// </summary>
        [Display(Name = "Status do agendamento")]
        public List<SchedulingStatus> Status { get; set; }
    }
}