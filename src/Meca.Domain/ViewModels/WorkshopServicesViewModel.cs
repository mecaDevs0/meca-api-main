using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class WorkshopServicesViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Serviço
        /// </summary>
        [Display(Name = "Serviço")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public BaseReferenceAuxViewModel Service { get; set; }
        /// <summary>
        /// Serviço rápido?
        /// </summary>
        [Display(Name = "Serviço rápido?")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public bool QuickService { get; set; }
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
        /// <summary>
        /// Foto (Nome retornado pela api de upload ex (3142315321.png))
        /// </summary>
        /// <exemple>323141235125.png</example>
        [Display(Name = "Foto do serviço")]
        [JsonConverter(typeof(PathImage))]
        public string Photo { get; set; }
        /// <summary>
        /// Tempo estimado
        /// </summary>
        [Display(Name = "Tempo estimado (em horas)")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double EstimatedTime { get; set; }
        /// <summary>
        /// Oficina
        /// </summary>
        [Display(Name = "Oficina")]
        public WorkshopAuxViewModel Workshop { get; set; }
    }
}