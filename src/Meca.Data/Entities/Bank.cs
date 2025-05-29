using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Bank : TEntity<Bank>
    {
        public string Code { get; set; }
        public string Name { get; set; }

    }
}