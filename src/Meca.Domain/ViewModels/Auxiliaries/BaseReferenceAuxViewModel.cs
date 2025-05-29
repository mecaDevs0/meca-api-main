using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class BaseReferenceAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}