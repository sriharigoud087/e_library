using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class BookRequests
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; } = null!;
        public string BookId { get; set; } = null!;

    }
}
