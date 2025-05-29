using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class WorkshopServices : TEntity<WorkshopServices>
    {
        public BaseReferenceAux Service { get; set; }
        public bool QuickService { get; set; }
        public double MinTimeScheduling { get; set; }
        public string Description { get; set; }
        public double EstimatedTime { get; set; }
        public WorkshopAux Workshop { get; set; }
        public string Photo { get; set; }
    }
}