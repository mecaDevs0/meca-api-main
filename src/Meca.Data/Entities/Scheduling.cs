using System.Collections.Generic;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Scheduling : TEntity<Scheduling>
    {
        public List<WorkshopServicesAux> WorkshopServices { get; set; }
        public VehicleAux Vehicle { get; set; }
        public string Observations { get; set; }
        public long Date { get; set; }
        public long? SuggestedDate { get; set; }
        public SchedulingStatus Status { get; set; }
        public ProfileAux Profile { get; set; }
        public WorkshopAux Workshop { get; set; }

        // Orçamento
        public long? BudgetApprovalDate { get; set; }
        public long? EstimatedTimeForCompletion { get; set; }
        public double? DiagnosticValue { get; set; }
        public List<string> BudgetImages { get; set; }
        public double? TotalValue { get; set; }
        public List<BudgetServicesAux> BudgetServices { get; set; }
        public List<BudgetServicesAux> MaintainedBudgetServices { get; set; }
        public List<BudgetServicesAux> ExcludedBudgetServices { get; set; }

        // Pagamento
        public long? PaymentDate { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }

        // Serviço
        public long? ServiceStartDate { get; set; }
        public long? ServiceEndDate { get; set; }
        public string ReasonDisapproval { get; set; }
        public List<string> ImagesDisapproval { get; set; }
        public string Dispute { get; set; }
        public List<string> ImagesDispute { get; set; }
        public bool FreeRepair { get; set; }
        public bool AwaitFreeRepairScheduling { get; set; }
        public bool HasEvaluated { get; set; }

        // Admin
        public bool? ServiceFinishedByAdmin { get; set; }
        public UserAdministratorAux UserAdministrator { get; set; }
        public List<BudgetServicesAux> BudgetServicesApprovedByAdmin { get; set; }
        public string PaymentIntentId { get; set; }

    }
}