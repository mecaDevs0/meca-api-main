using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class WorkshopAgenda : TEntity<WorkshopAgenda>
    {
        public WorkshopAgendaAux Sunday { get; set; }
        public WorkshopAgendaAux Monday { get; set; }
        public WorkshopAgendaAux Tuesday { get; set; }
        public WorkshopAgendaAux Wednesday { get; set; }
        public WorkshopAgendaAux Thursday { get; set; }
        public WorkshopAgendaAux Friday { get; set; }
        public WorkshopAgendaAux Saturday { get; set; }
        public WorkshopAux Workshop { get; set; }
    }
}