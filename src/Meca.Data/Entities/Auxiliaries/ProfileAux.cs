using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class ProfileAux
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Photo { get; set; }
    }
}