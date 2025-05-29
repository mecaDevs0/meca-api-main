
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class PushViewModel
    {

        /// <summary>
        /// Identificador do Device Onesignal
        /// </summary>
        /// <example></example>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Identificador do Device Onesignal")]
        public string DeviceId { get; set; }
        /// <summary>
        /// Registrar (true para registrar | false para remover)
        /// </summary>
        [Display(Name = "Registrar ")]
        public bool IsRegister { get; set; }
    }
}