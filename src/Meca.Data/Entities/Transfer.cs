using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Transfer : TEntity<Transfer>
    {
        public string InvoiceId { get; set; }
        public string AccountId { get; set; }
        public double Value { get; set; }
        public double SourceValue { get; set; }
        public double Fees { get; set; }
        public RecipientType RecipientType { get; set; }
        public string TransferId { get; set; }
    }
}