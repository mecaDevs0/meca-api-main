using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Filters;

namespace Meca.Domain.ViewModels.Filters
{
    public class ProfileFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Nome completo
        /// </summary>
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }
    }
}