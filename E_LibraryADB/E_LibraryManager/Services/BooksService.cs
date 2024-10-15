using Microsoft.Extensions.Options;
using E_LibraryManager.Models;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Common.IntitialData;
using MongoDB.Bson;
using E_LibraryManager.ViewModels;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using System.Transactions;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Data.Common;

namespace E_LibraryManager.Services
{
    public class BooksService
    {
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly BookTransactionsService _bookTransactionsService;




        public BooksService(IOptions<DatabaseSettings> bookStoreDatabaseSettings, UserService userService, EmailService emailService, BookTransactionsService bookTransactionsService)
        {
            var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<Book>("Books");

            _userService = userService;
            _emailService = emailService;
            _bookTransactionsService = bookTransactionsService;
        }
        public async Task<List<Book>> GetBooksAsync()
        {
            return await _booksCollection.Find(x => true).ToListAsync();
        }
        public async Task<Book> GetBookAsync(string id)
        {
            return await _booksCollection.Find(x => x.BookId == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Book book)
        {

            var users = await _userService.GetAsync();

            var emails = users.Select(x => x.Email).ToArray();

            var subject = $"New Arrival status of book - {book.Title}";
            var body = $"<p>The book with Title <b>{book.Title}</b> by the author <b>{book.Author}</b> recently added in the Library. Kindly visit the library to know more about the book.</p><p>Thank you</p>";

            if (emails != null)
                _emailService.SendEmail(new Message(emails, subject, body, new string[] { }, new string[] { }, true));


            await _booksCollection.InsertOneAsync(book);
        }

        public async Task<ReplaceOneResult> UpdateAsync(string id, Book updatedBook)
        {
            return await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);
        }

        public async Task RemoveAsync(string id)
        {
            await _booksCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<BookRecord> GetTransaction(string transactionId)
        {
            var transaction = await _bookTransactionsService.GetTransaction(transactionId);
            var book = await GetBookAsync(transaction.BookId);
            return BookRecord.MergeBookRecord(book, transaction);
            //return await _booksCollection.Aggregate().Unwind<Book, BookRecord>(x => x.bookTransactions).Match(x => x.bookTransactions.TransactionId == transactionId).FirstOrDefaultAsync();
        }

        public async Task<List<BookRecord>> AdminIssuedBooks()
        {
            var issuedBooks = new List<BookRecord>();
            var transactions = await _bookTransactionsService.AdminIssuedBooks();
            var bookIds = transactions.Select(x => x.BookId).ToList();

            var filter = Builders<Book>.Filter.In(x => x.BookId, bookIds);
            var books = await _booksCollection.Aggregate()
                .Match(filter)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                var book = books.FirstOrDefault(x => x.BookId == transaction.BookId);
                if (book != null)
                {
                    issuedBooks.Add(BookRecord.MergeBookRecord(book, transaction));
                }
            }

            return issuedBooks;
        }

        public async Task<List<BookRecord>> UserIssuedBooks(string userId)
        {
            var issuedBooks = new List<BookRecord>();
            var transactions = await _bookTransactionsService.UserIssuedBooks(userId);
            var bookIds = transactions.Select(x => x.BookId).ToList();

            var filter = Builders<Book>.Filter.In(x => x.BookId, bookIds);
            var books = await _booksCollection.Aggregate()
                .Match(filter)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                var book = books.FirstOrDefault(x => x.BookId == transaction.BookId);
                if (book != null)
                {
                    issuedBooks.Add(BookRecord.MergeBookRecord(book, transaction));
                }
            }

            return issuedBooks;
        }


        public async Task UpdateTransaction(TransactionVM transaction, bool isRenew)
        {
            var book = await GetBookAsync(transaction.BookId);
            var days = (int)(DateTime.Now - transaction.DueDate).TotalDays;
            if (days < 0)
            {
                  days = 0;
            }
            if (isRenew)
            {
                transaction.CheckInDateTime = DateTime.Now;
                transaction.DueDate = DateTime.Now.AddDays(15);
                transaction.IsActive = true;
                book.isAvailable = false;
                book.Status = BookStatus.NotAvailable.ToString();
            }
            else
            {
                transaction.CheckOutDateTime = DateTime.Now;
                transaction.IsActive = false;
                book.Status = BookStatus.Available.ToString(); ;
            }
            transaction.Status = isRenew ? BookStatus.Renewed.ToString() : BookStatus.Returned.ToString();
            transaction.RenewalCount = isRenew ? transaction.RenewalCount + 1 : transaction.RenewalCount;
            var transactionBook = _bookTransactionsService.GetTransaction(transaction.TransactionId);
            var bookTransaction = new BookTransaction()
            {
                TransactionId = transaction.TransactionId,
                BookId = transaction.BookId,
                UserId = transaction.UserId,
                CheckInDateTime = transaction.CheckInDateTime,
                CheckOutDateTime = transaction.CheckOutDateTime,
                DueDate = transaction.DueDate,
                Status = transaction.Status,
                RenewalCount = transaction.RenewalCount,
                IsActive = transaction.IsActive,
                TransactionDate = DateTime.Now,
                Id= transactionBook.Result.Id
            };
            var res = await _bookTransactionsService.UpdateTransactionAsync(bookTransaction.Id, bookTransaction);
        }
        public async Task<bool> CheckOutBooks(List<BookTransaction> bookTransactions)
        {
            foreach (var transaction in bookTransactions)
            {
                var bookRecord = await GetTransaction(transaction.TransactionId);
                var book = await GetBookAsync(transaction.BookId);
                var user = await _userService.GetAsync(transaction.UserId);
                book.isAvailable = true;
                book.Status = BookStatus.Available.ToString();
                var res = await UpdateAsync(book.Id, book);
                if (book == null || user == null || bookRecord == null)
                {
                    continue;
                }
                else
                {

                    var transactionVM = new TransactionVM()
                    {
                        TransactionId = transaction.TransactionId,
                        BookId = transaction.BookId,
                        UserId = transaction.UserId,
                        CheckInDateTime = transaction.CheckInDateTime,
                        CheckOutDateTime = transaction.CheckOutDateTime,
                        DueDate = transaction.DueDate,
                        Penalty = null, // Set if applicable
                        Status = BookStatus.Returned.ToString(),
                        RenewalCount = transaction.RenewalCount,
                        IsActive = false
                    };
                    await CheckoutAsync(transactionVM, bookRecord, book, user);
                }

            }
            return true;
        }
        public async Task CheckoutAsync(TransactionVM transaction, BookRecord bookRecord, Book book, User user,bool renew=false)
        {
            var isRenew = renew;
            transaction.CheckInDateTime = bookRecord.bookTransactions.CheckInDateTime;
            transaction.DueDate = bookRecord.bookTransactions.DueDate;
            transaction.RenewalCount = bookRecord.bookTransactions.RenewalCount;
            await UpdateTransaction(transaction, isRenew);

            bookRecord = await GetTransaction(transaction.TransactionId);

            var emailBody = PrepareEmailBody.GetEmailBody(new MailBodyProperties()
            {
                BookId = transaction.BookId,
                Status = BookStatus.Returned.ToString(),
                UserId = user.UserId,
                DueDate = bookRecord.bookTransactions.DueDate,
                DateTaken = bookRecord.bookTransactions.CheckInDateTime,
                ISBN = book.ISBN,
                Title = book.Title,
                Description = book.Description
            });
            _emailService.SendEmail(new Message(new string[] { user.Email }, "Boossk Returned", emailBody, new string[] { }, new string[] { }, true));
        }
        public async Task<long> GetTotalBookCount()
        {
            return await _booksCollection.CountDocumentsAsync(new BsonDocument());
        }
    }

}
