using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class WorkshopAgendaAux
    {
        public bool Open { get; set; }
        public string StartTime { get; set; }
        public string ClosingTime { get; set; }
        public string StartOfBreak { get; set; }
        public string EndOfBreak { get; set; }
    }
}