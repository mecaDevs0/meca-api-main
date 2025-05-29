using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class FaqFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Pergunta
        /// </summary>
        [Display(Name = "Pergunta")]
        public string Question { get; set; }
    }
}