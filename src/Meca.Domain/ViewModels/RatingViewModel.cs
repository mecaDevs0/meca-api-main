using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class RatingViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Qualidade do atendimento
        /// </summary>
        [Display(Name = "Qualidade do atendimento")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Range(0, 5, ErrorMessage = DefaultMessages.RatingInvalid)]
        public int AttendanceQuality { get; set; }
        /// <summary>
        /// Qualidade do serviço
        /// </summary>
        [Display(Name = "Qualidade do serviço")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Range(0, 5, ErrorMessage = DefaultMessages.RatingInvalid)]
        public int ServiceQuality { get; set; }
        /// <summary>
        /// Custo benefício
        /// </summary>
        [Display(Name = "Custo benefício")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Range(0, 5, ErrorMessage = DefaultMessages.RatingInvalid)]
        public int CostBenefit { get; set; }
        /// <summary>
        /// Descrição
        /// </summary>
        [Display(Name = "Descrição")]
        public string Observations { get; set; }
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Usuário
        /// </summary>
        [Display(Name = "Usuário")]
        [IsReadOnly]
        public ProfileAuxViewModel Profile { get; set; }
        /// <summary>
        /// Lista de serviços
        /// </summary>
        [Display(Name = "Lista de serviços")]
        [IsReadOnly]
        public List<BudgetServicesAuxViewModel> BudgetServices { get; set; }
        /// <summary>
        /// Oficina
        /// </summary>
        [Display(Name = "Oficina")]
        [IsReadOnly]
        public WorkshopAuxViewModel Workshop { get; set; }
        /// <summary>
        /// Veículo
        /// </summary>
        [Display(Name = "Veículo")]
        [IsReadOnly]
        public VehicleAuxViewModel Vehicle { get; set; }
        /// <summary>
        /// Média de avaliação
        /// </summary>
        [Display(Name = "Média de avaliação")]
        [IsReadOnly]
        public int RatingAverage { get; set; }
    }
}