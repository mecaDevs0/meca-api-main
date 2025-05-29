using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class RatingFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// ID da oficina
        /// </summary>
        [Display(Name = "ID da oficina")]
        public string WorkshopId { get; set; }
    }
}