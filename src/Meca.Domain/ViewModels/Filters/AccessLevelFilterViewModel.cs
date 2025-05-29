using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class AccessLevelFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Nome do perfil de acesso
        /// </summary>
        [Display(Name = "Nome do perfil de acesso ")]
        public string Name { get; set; }

        /// <summary>
        /// É um master
        /// </summary>
        [Display(Name = "É um master")]
        public bool? IsDefault { get; set; }
    }
}