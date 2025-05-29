using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class WithDraw : TEntity<WithDraw>
    {
        public string AccountId { get; set; }
        public double Value { get; set; }
        public string WithDrawId { get; set; }
        public RecipientType RecipientType { get; set; }
    }
}