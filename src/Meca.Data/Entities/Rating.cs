using System.Collections.Generic;
using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Rating : TEntity<Rating>
    {
        public int AttendanceQuality { get; set; }
        public int ServiceQuality { get; set; }
        public int CostBenefit { get; set; }
        public string Observations { get; set; }
        public ProfileAux Profile { get; set; }
        public List<BudgetServicesAux> BudgetServices { get; set; }
        public WorkshopAux Workshop { get; set; }
        public string SchedulingId { get; set; }
        public VehicleAux Vehicle { get; set; }
        public int RatingAverage { get; set; }
    }
}