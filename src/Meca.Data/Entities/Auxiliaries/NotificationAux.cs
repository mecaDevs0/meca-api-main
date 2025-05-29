using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class NotificationAux
    {
        public string ReferenceId { get; set; }
        public string ReferenceName { get; set; }
        public TypeProfile TypeReference { get; set; }
    }
}