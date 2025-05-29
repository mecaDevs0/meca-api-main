using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class ItemMenuRule
    {
        public MenuItem MenuItem { get; set; }
        public int Position { get; set; }
        [BsonIgnoreIfNull]
        public SubMenuItem? SubMenu { get; set; }
        [BsonIgnoreIfNull]
        public bool? Access { get; set; } = true;
        [BsonIgnoreIfNull]
        public bool? EnableDisable { get; set; } = true;
        [BsonIgnoreIfNull]
        public bool? Edit { get; set; } = true;
        [BsonIgnoreIfNull]
        public bool? Write { get; set; } = true;
        [BsonIgnoreIfNull]
        public bool? Delete { get; set; } = true;
        [BsonIgnoreIfNull]
        public bool? Export { get; set; } = true;
    }
}