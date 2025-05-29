using System.Collections.Generic;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class FinancialHistory : TEntity<FinancialHistory>
    {
        public string InvoiceId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string CreditCardId { get; set; }
        public int Installment { get; set; } = 1;
        public ProfileAux Profile { get; set; }
        public WorkshopAux Workshop { get; set; }
        public VehicleAux Vehicle { get; set; }
        public List<WorkshopServicesAux> WorkshopServices { get; set; }
        public string SchedulingId { get; set; }

        /*VALORES*/
        public double Value { get; set; }
        public double? NetValue { get; set; }
        public double? ProcessingValue { get; set; }
        public long? PaymentDate { get; set; }
        public long? ReleasedDate { get; set; }
        public long? ExpiredDate { get; set; }
        public long? RefundDate { get; set; }
        public double? ReversedValue { get; set; }
        public double? GatewayValue { get; set; }
        public double? PlatformFee { get; set; }
        public double? PlatformValue { get; set; }
        public double? MechanicalNetValue { get; set; }

        /*TRANSAÇÃO*/
        public string PixQrCode { get; set; }
        public string PixQrCodeTxt { get; set; }

    }
}