using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Services.Stripe.Core3.Models;

namespace Meca.Domain.ViewModels
{
    public class SchedulingViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Lista de serviços
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Lista de serviços")]
        public List<WorkshopServicesAuxViewModel> WorkshopServices { get; set; }
        /// <summary>
        /// Veículo
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Veículo")]
        public VehicleAuxViewModel Vehicle { get; set; }
        /// <summary>
        /// Observações
        /// </summary>
        [Display(Name = "Observações")]
        public string Observations { get; set; }
        /// <summary>
        /// Data do agendamento
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Data do agendamento")]
        public long Date { get; set; }
        /// <summary>
        /// Data sugerida pela oficina para o agendamento
        /// </summary>
        [Display(Name = "Data sugerida pela oficina para o agendamento")]
        public long? SuggestedDate { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Status")]
        [IsReadOnly]
        public SchedulingStatus Status { get; set; }
        /// <summary>
        /// Usuário
        /// </summary>
        [Display(Name = "Usuário")]
        public ProfileAuxViewModel Profile { get; set; }
        /// <summary>
        /// Oficina
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Oficina")]
        public WorkshopAuxViewModel Workshop { get; set; }

        // Orçamento
        /// <summary>
        /// Data da aprovação do orçamento
        /// </summary>
        [Display(Name = "Data da aprovação do orçamento")]
        [IsReadOnly]
        public long? BudgetApprovalDate { get; set; }
        /// <summary>
        /// Prazo estimado para conclusão
        /// </summary>
        [Display(Name = "Prazo estimado para conclusão")]
        public long? EstimatedTimeForCompletion { get; set; }
        /// <summary>
        /// Valor do diagnóstico
        /// </summary>
        [Display(Name = "Valor do diagnóstico")]
        public double? DiagnosticValue { get; set; }
        /// <summary>
        /// Imagens enviadas pelo estabelecimento referentes ao orçamento
        /// </summary>
        [Display(Name = "Imagens enviadas pelo estabelecimento referentes ao orçamento")]
        [JsonConverter(typeof(PathImage))]
        public List<string> BudgetImages { get; set; }
        /// <summary>
        /// Valor total
        /// </summary>
        [Display(Name = "Valor total")]
        [IsReadOnly]
        public double? TotalValue { get; set; }
        /// <summary>
        /// Serviços do orçamento
        /// </summary>
        [Display(Name = "Serviços do orçamento")]
        [IsReadOnly]
        public List<BudgetServicesAuxViewModel> BudgetServices { get; set; }
        /// <summary>
        /// Serviços do orçamento mantidos
        /// </summary>
        [Display(Name = "Serviços do orçamento mantidos")]
        [IsReadOnly]
        public List<BudgetServicesAuxViewModel> MaintainedBudgetServices { get; set; }
        /// <summary>
        /// Serviços do orçamento excluidos
        /// </summary>
        [Display(Name = "Serviços do orçamento excluidos")]
        [IsReadOnly]
        public List<BudgetServicesAuxViewModel> ExcludedBudgetServices { get; set; }

        // Pagamento

        /// <summary>
        /// Data do pagamento
        /// </summary>
        [Display(Name = "Data do pagamento")]
        [IsReadOnly]
        public long? PaymentDate { get; set; }
        /// <summary>
        /// Status do pagamento
        /// </summary>
        [Display(Name = "Status do pagamento")]
        [IsReadOnly]
        public PaymentStatus? PaymentStatus { get; set; }

        // Serviço

        /// <summary>
        /// Data de ínicio do serviço
        /// </summary>
        [Display(Name = "Data de ínicio do serviço")]
        [IsReadOnly]
        public long? ServiceStartDate { get; set; }
        /// <summary>
        /// Data de conclusão do serviço
        /// </summary>
        [Display(Name = "Data de conclusão do serviço")]
        [IsReadOnly]
        public long? ServiceEndDate { get; set; }
        /// <summary>
        /// Motivo da reprovação
        /// </summary>
        [Display(Name = "Motivo da reprovação")]
        public string ReasonDisapproval { get; set; }
        /// <summary>
        /// Imagens (da reprovação)
        /// </summary>
        [Display(Name = "Imagens (da reprovação)")]
        [JsonConverter(typeof(PathImage))]
        public List<string> ImagesDisapproval { get; set; }
        /// <summary>
        /// Contestação
        /// </summary>
        [Display(Name = "Contestação")]
        public string Dispute { get; set; }
        /// <summary>
        /// Imagens (da contestação)
        /// </summary>
        [Display(Name = "Imagens (da contestação)")]
        [JsonConverter(typeof(PathImage))]
        public List<string> ImagesDispute { get; set; }
        /// <summary>
        /// Reparo gratuito?
        /// </summary>
        [Display(Name = "Reparo gratuito?")]
        [IsReadOnly]
        public bool FreeRepair { get; set; } = false;
        /// <summary>
        /// Aguardando agendamento de reparo gratuito?
        /// </summary>
        [Display(Name = "Aguardando agendamento de reparo gratuito?")]
        [IsReadOnly]
        public bool AwaitFreeRepairScheduling { get; set; }
        /// <summary>
        /// Agendamento já avaliado?
        /// </summary>
        [Display(Name = "Agendamento já avaliado?")]
        [IsReadOnly]
        public bool HasEvaluated { get; set; }

        // Admin

        /// <summary>
        /// Status alterado pelo administrador?
        /// </summary>
        [Display(Name = "Status alterado pelo administrador?")]
        [IsReadOnly]
        public bool? ServiceFinishedByAdmin { get; set; }
        /// <summary>
        /// Administrador
        /// </summary>
        [Display(Name = "Administrador")]
        [IsReadOnly]
        public UserAdministratorAuxViewModel UserAdministrator { get; set; }
        /// <summary>
        /// Serviços aprovados pelo administrador
        /// </summary>
        [Display(Name = "Serviços aprovados pelo administrador")]
        [IsReadOnly]
        public List<BudgetServicesAuxViewModel> BudgetServicesApprovedByAdmin { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        [Display(Name = "Data da última atualização")]
        [IsReadOnly]
        public long? LastUpdate { get; set; }
        /// <summary>
        /// Regra de split
        /// </summary>
        [Display(Name = "Regra de split")]
        [IsReadOnly]
        public StripePaymentIntentSplitModel DataTransfer { get; set; }
    }
}