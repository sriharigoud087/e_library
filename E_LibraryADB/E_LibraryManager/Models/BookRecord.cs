using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_LibraryManager.Models
{
    public class BookRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string BookId { set; get; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public string Category { set; get; } = null!;
        public int PublishedYear { get; set; }
        public string Author { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool isAvailable { get; set; }
        public BookTransaction bookTransactions { set; get; } = null!;
        public List<BookRequests> bookRequests { set; get; } = null!;

        public byte[]? FileData { get; set; } = null!; // Add this property for storing file data
        public string? FileType { get; set; } = null!;
        public static BookRecord MergeBookRecord(Book book, BookTransaction transaction)
        {
            return new BookRecord
            {
                Id = book.Id,
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Image = book.Image,
                ISBN = book.ISBN,
                Category = book.Category,
                PublishedYear = book.PublishedYear,
                Author = book.Author,
                Status = book.Status,
                isAvailable = book.isAvailable,
                bookTransactions = transaction,
                FileData = book?.FileData,
                FileType = book?.FileType
            };
        }
    }
  
}
