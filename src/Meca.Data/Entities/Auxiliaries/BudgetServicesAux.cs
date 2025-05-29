using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class BudgetServicesAux
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
    }
}