using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Vehicle : TEntity<Vehicle>
    {
        public string Plate { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public double? Km { get; set; }
        public string Color { get; set; }
        public string Year { get; set; }
        public long? LastRevisionDate { get; set; }
        public ProfileAux Profile { get; set; }
    }
}