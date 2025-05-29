using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class UserAdministratorFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// ID do perfil de acesso
        /// </summary>
        [Display(Name = "ID do perfil de acesso ")]
        public string AccessLevelId { get; set; }
    }
}