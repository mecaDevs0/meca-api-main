using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class WorkshopServicesAux
    {
        public string Id { get; set; }
        public BaseReferenceAux Service { get; set; }
        public double MinTimeScheduling { get; set; }
        public string Description { get; set; }
    }
}