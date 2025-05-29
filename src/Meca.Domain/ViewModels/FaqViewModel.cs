using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels
{
    public class FaqViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Pergunta
        /// </summary>
        [Display(Name = "Pergunta")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Question { get; set; }
        /// <summary>
        /// Resposta
        /// </summary>
        [Display(Name = "Resposta")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Response { get; set; }
    }
}