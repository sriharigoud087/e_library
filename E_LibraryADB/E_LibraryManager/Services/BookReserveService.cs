using Microsoft.Extensions.Options;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Models;

namespace E_LibraryManager.Services
{
    public class BookReserveService
    {
        private readonly IMongoCollection<BookReservations> _bookReservationsCollection;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public BookReserveService(IOptions<DatabaseSettings> bookStoreDatabaseSettings, UserService userService, EmailService emailService)
        {
            var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _bookReservationsCollection = mongoDatabase.GetCollection<BookReservations>("BookReservations");

            _userService = userService;
            _emailService = emailService;
        }
        public async Task<List<BookReservations>> GetBookReservationsAsync()
        {
            return await _bookReservationsCollection.Find(x => true).ToListAsync();
        }
        public async Task<BookReservations> GetBookReservationsByUserIdAsync(string id)
        {
            return await _bookReservationsCollection.Find(x => x.UserId == id).FirstOrDefaultAsync();
        }
        public async Task<BookReservations> GetBookReservationsByBookIdAsync(string id)
        {
            return await _bookReservationsCollection.Find(x => x.BookId == id).FirstOrDefaultAsync();
        }
        public async Task CreateBookReservationsAsync(BookReservations book)
        {
            await _bookReservationsCollection.InsertOneAsync(book);
        }

        public async Task UpdateBookReservationsAsync(string id, BookReservations bookTransaction)
        {
            await _bookReservationsCollection.ReplaceOneAsync(x => x.Id == id, bookTransaction);
        }

        public async Task RemoveAsync(string id)
        {
            await _bookReservationsCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<BookReservations> GetBookReservationById(string id)
        {
            return await _bookReservationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<BookReservations>> AdminBookReservations()
        {
            var transactions = await _bookReservationsCollection.Find(x => true).ToListAsync();
            return transactions;
        }

        public async Task<List<BookReservations>> UserBookReservations(string userId)
        {
            var transactions = await _bookReservationsCollection.Find(x => x.UserId == userId).ToListAsync();
            return transactions;
        }
        public  List<BookReservations> GetBookReservationsByBookIdWithTime(string bookId)
        {
            return  _bookReservationsCollection.Find(x => x.BookId == bookId).ToList().OrderByDescending(x => x.ReservedTime).ToList();
        }
    }
}
