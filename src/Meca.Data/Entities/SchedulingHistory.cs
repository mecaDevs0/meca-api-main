using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class SchedulingHistory : TEntity<SchedulingHistory>
    {
        public SchedulingStatusTitle StatusTitle { get; set; }
        public SchedulingStatus Status { get; set; }
        public string Description { get; set; }
        public string SchedulingId { get; set; }
    }
}