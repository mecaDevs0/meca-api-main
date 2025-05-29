using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]

    public class CreditCard : TEntity<CreditCard>
    {
        public string ProfileId { get; set; }
        public string TokenCard { get; set; }

    }
}