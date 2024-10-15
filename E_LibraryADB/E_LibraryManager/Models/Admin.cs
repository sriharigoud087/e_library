using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Password { get; set; } = null!;
        public string? Email { get; set; } = null!;
    }
}
