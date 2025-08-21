using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    public class Bank : TEntity<Bank>
    {
        [BsonElement("code")]
        public string Code { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
    }
}