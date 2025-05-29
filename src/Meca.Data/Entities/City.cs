using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class City : UtilityFramework.Infra.Core3.MongoDb.Data.Modelos.City
    {
        public string StateUf { get; set; }
    }
}