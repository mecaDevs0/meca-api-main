using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class BaseReferenceAux
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}