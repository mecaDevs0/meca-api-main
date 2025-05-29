using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class BaseReferenceAux
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}