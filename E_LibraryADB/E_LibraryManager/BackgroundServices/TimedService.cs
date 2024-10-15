using Microsoft.Extensions.Options;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Models;
using E_LibraryManager.Services;
using E_LibraryManager.ViewModels;
using System.Transactions;

namespace E_LibraryManager.BackgroundServices
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BookTransactionsService _bookTransactionsService;
        private readonly BookReserveService _bookReserveService;
        private readonly UserService _userService;
        private readonly BooksService _booksService;
        private readonly EmailService _emailService;
        private readonly IMongoCollection<BookTransaction> _bookTransactionCollection;
        private readonly IMongoCollection<BookReservations> _bookReservationsCollection;

        private Timer? _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<DatabaseSettings> bookStoreDatabaseSettings, IServiceScopeFactory serviceScopeFactory)
        {
            var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);

            _bookTransactionCollection = mongoDatabase.GetCollection<BookTransaction>("BookTransaction");
            _bookReservationsCollection = mongoDatabase.GetCollection<BookReservations>("BookReservations");

            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _bookTransactionsService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<BookTransactionsService>();
            _booksService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<BooksService>();
            _bookReserveService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<BookReserveService>();
            _userService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<UserService>();
            _emailService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<EmailService>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            var currentDate = DateTime.Now;

            var bookTransactions = _bookTransactionCollection.Find(x => x.IsActive == true).ToList();

            var filteredTransactions = bookTransactions.Where(x => DateTime.Now > x.CheckInDateTime.AddDays(15)).ToList();
            await _booksService.CheckOutBooks(filteredTransactions);

            foreach (var item in filteredTransactions)
            {
                var reservations = _bookReservationsCollection.Find(x => x.BookId == item.BookId).ToList().OrderByDescending(x => x.ReservedTime).ToList();
                if (reservations.Count > 0)
                {
                    foreach (var reservation in reservations)
                    {
                        var transactions = _bookTransactionsService.UserIssuedBooks(reservation.UserId).Result;
                        var user = _userService.GetAsync(reservation.UserId);
                        if (transactions.Count >= 3)
                        {
                            continue;
                        }
                        else
                        {
                            await _bookTransactionsService.CreateTransactionAsync(new BookTransaction()
                            {
                                UserId = reservation.UserId,
                                BookId = reservation.BookId,
                                CheckInDateTime = DateTime.Now,
                                DueDate = DateTime.Now.AddDays(15),
                                IsActive = true,
                                RenewalCount = 0,
                                TransactionDate = DateTime.Now,
                                Status = "Issued"
                            });
                            var book = await _booksService.GetBookAsync(reservation.BookId);
                            book.Status = BookStatus.NotAvailable.ToString();
                            book.isAvailable = false;
                            await _booksService.UpdateAsync(book.Id, book);
                            await _bookReserveService.RemoveAsync(reservation.Id);
                            var emailBody = PrepareEmailBody.GetEmailBody(new MailBodyProperties()
                            {
                                BookId = book.BookId,
                                Status = BookStatus.Requested.ToString(),
                                UserId = user.Result.UserId,
                                ISBN = book.ISBN,
                                Title = book.Title,
                                Description = book.Description,
                            });
                            _emailService.SendEmail(new Message(new string[] { user.Result.Email }, "Book Issued", emailBody, new string[] { }, new string[] { }, true));
                        }
                    }
                }
            }

            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
