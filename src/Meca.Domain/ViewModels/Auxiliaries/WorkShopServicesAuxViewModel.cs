using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class WorkshopServicesAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Id { get; set; }
        /// <summary>
        /// Serviço
        /// </summary>
        [Display(Name = "Serviço")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public BaseReferenceAuxViewModel Service { get; set; }
        /// <summary>
        /// Antecedência mínima para agendamento
        /// </summary>
        [Display(Name = "Antecedência mínima para agendamento (em horas)")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double MinTimeScheduling { get; set; }
        /// <summary>
        /// Descrição
        /// </summary>
        [Display(Name = "Descrição")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Description { get; set; }
    }
}