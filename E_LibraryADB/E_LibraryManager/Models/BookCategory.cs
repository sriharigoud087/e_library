using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class BookCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class BookCategoryVM
    {
        public string CategoryName { get; set; } = null!;
    }

}
