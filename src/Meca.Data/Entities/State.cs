using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class State : UtilityFramework.Infra.Core3.MongoDb.Data.Modelos.State
    {
        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }
}