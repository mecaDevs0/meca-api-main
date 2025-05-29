using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class AgendaAux : TEntity<AgendaAux>
    {
        public string WorkshopId { get; set; }
        public long Date { get; set; }
    }
}