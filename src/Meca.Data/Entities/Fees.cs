using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Fees : TEntity<Fees>
    {
        public double PlatformFee { get; set; }
    }
}