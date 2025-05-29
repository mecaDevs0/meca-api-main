using Meca.Data.Entities.Auxiliaries;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class UserAdministrator : TEntity<UserAdministrator>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsDefault { get; set; }
        public BaseReferenceAux AccessLevel { get; set; }
    }
}