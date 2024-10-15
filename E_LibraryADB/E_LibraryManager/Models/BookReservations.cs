using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class BookReservations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? BookId { get; set; }
        public string? UserId { get; set; }
        public DateTime? ReservedTime { get; set; }

    }
}
