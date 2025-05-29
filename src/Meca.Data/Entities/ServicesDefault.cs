using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class ServicesDefault : TEntity<ServicesDefault>
    {
        public string Name { get; set; }
        public bool QuickService { get; set; }
        public double MinTimeScheduling { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
    }
}