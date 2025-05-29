using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class SchedulingHistoryFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
    }
}