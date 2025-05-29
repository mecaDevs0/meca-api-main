using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class UserAdministratorAux
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}