using Microsoft.Extensions.Options;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Models;
using E_LibraryManager.Utilties;
using E_LibraryManager.ViewModels;
using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace E_LibraryManager.Services
{
    public class BookTransactionsService
    {
        private readonly IMongoCollection<BookTransaction> _bookTransactionCollection;
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly BooksService _booksService;


        public BookTransactionsService(IOptions<DatabaseSettings> bookStoreDatabaseSettings, UserService userService, EmailService emailService)
        {
            var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _bookTransactionCollection = mongoDatabase.GetCollection<BookTransaction>("BookTransaction");

            _userService = userService;
            _emailService = emailService;
            //_booksService = booksService;
        }
        public async Task<List<BookTransaction>> GetBookTransactionsAsync()
        {
            return await _bookTransactionCollection.Find(x => true).ToListAsync();
        }
        public async Task<BookTransaction> GetBookTransactionsAsync(string id)
        {
            return await _bookTransactionCollection.Find(x => x.TransactionId == id).FirstOrDefaultAsync();
        }

        public async Task CreateTransactionAsync(BookTransaction book)
        {
            await _bookTransactionCollection.InsertOneAsync(book);
        }

        public async Task<ReplaceOneResult> UpdateTransactionAsync(string id, BookTransaction bookTransaction)
        {
            return await _bookTransactionCollection.ReplaceOneAsync(x => x.Id == id, bookTransaction);
        }

        public async Task RemoveAsync(string id)
        {
            await _bookTransactionCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<BookTransaction> GetTransaction(string transactionId)
        {
            return await _bookTransactionCollection.Find(x => x.TransactionId == transactionId).FirstOrDefaultAsync();
            //return await _bookTransactionCollection.Aggregate().Unwind<BookTransaction, BookRecord>(x => x.bookTransactions).Match(x => x.bookTransactions.TransactionId == transactionId).FirstOrDefaultAsync();
        }

        public async Task<List<BookTransaction>> AdminIssuedBooks()
        {
            var transactions = await _bookTransactionCollection.Find(x => true).ToListAsync();
           
            return transactions;
        }

        public async Task<List<BookTransaction>> UserIssuedBooks(string userId)
        {
            var transactions = await _bookTransactionCollection.Find(x => x.UserId == userId).ToListAsync();
            return transactions;
        }
       
        public string PrepareEmailBody(MailBodyProperties bodyProperties)
        {
            var templateManager = new EmailTemplateManager();
            var body = templateManager.GetTemplate(EmailTemplate.BookNotification);
            body = body.Replace("##USERID", bodyProperties.UserId);
            body = body.Replace("##BOOKID", bodyProperties.BookId);
            body = body.Replace("##ISBN", bodyProperties.ISBN);
            body = body.Replace("##STATUS", bodyProperties.Status);
            body = body.Replace("##TITLE", bodyProperties.Title);
            body = body.Replace("##DUEDATE", bodyProperties.DueDate.ToString());
            body = body.Replace("##DATETAKEN", bodyProperties.DateTaken.ToString());
            body = body.Replace("##DESCRIPTION", bodyProperties.Description);
            return body;
        }
    }
}
