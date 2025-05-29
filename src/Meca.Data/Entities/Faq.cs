using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Faq : TEntity<Faq>
    {
        public string Question { get; set; }
        public string Response { get; set; }
    }
}