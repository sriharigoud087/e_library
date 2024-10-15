using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_LibraryManager.Models
{
    public class Author
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int AuthorId { set; get; }
        public string Name { get; set; } = null!;
    }


    public class AuthorVM
    {
        public string Name { get; set; } = null!;
    }
}
