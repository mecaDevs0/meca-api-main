using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class AccessLevel : TEntity<AccessLevel>
    {
        public string Name { get; set; }
        public List<ItemMenuRule> Rules { get; set; } = [];
        public bool IsDefault { get; set; }
    }
}