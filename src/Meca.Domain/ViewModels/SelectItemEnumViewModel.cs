
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class SelectItemEnumViewModel
    {
        /// <summary>
        /// Nome
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }
        /// <summary>
        /// Valor
        /// </summary>
        [Display(Name = "Valor")]
        public int Value { get; set; }
    }
}