using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class WorkshopAux
    {
        public string Id { get; set; }
        public string Photo { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cnpj { get; set; }

        /*ADDRESS INFO*/
        public string ZipCode { get; set; }
        public string StreetAddress { get; set; }
        public string Number { get; set; }
        public string CityName { get; set; }
        public string CityId { get; set; }
        public string StateName { get; set; }
        public string StateUf { get; set; }
        public string StateId { get; set; }
        public string Neighborhood { get; set; }
        public string Complement { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}