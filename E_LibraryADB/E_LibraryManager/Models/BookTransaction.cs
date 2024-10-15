using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class BookTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TransactionId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string BookId { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime CheckInDateTime { set; get; }
        public DateTime CheckOutDateTime { set; get; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public bool IsActive { set; get; }
        public int RenewalCount { get; set; }

    }
}
