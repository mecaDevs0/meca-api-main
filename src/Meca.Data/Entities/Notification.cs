using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Notification : TEntity<Notification>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceName { get; set; }
        public TypeProfile TypeReference { get; set; }
        public long? DateRead { get; set; }

        // Custom
        public WorkshopAux Workshop { get; set; }
        public ProfileAux Profile { get; set; }
        public string SchedulingId { get; set; }
    }
}