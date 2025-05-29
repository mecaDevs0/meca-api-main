using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels
{
    public class FeesViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Taxa da plataforma
        /// </summary>
        [Display(Name = "Taxa da plataforma")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double PlatformFee { get; set; }
    }
}