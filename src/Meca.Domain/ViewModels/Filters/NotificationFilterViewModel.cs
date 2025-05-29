using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;

namespace Meca.Domain.ViewModels.Filters
{
    public class NotificationFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Tipo de usuário
        /// </summary>
        [Display(Name = "Tipo de usuário")]
        public TypeProfile? TypeReference { get; set; }
        /// <summary>
        /// Marcar como visualizado
        /// </summary>
        [Display(Name = "Marcar como visualizado")]
        public bool SetRead { get; set; }
        /// <summary>
        /// ID do usuário
        /// </summary>
        [Display(Name = "ID do usuário")]
        public string UserId { get; set; }
    }
}