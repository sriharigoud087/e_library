using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using E_LibraryManager.Models;
using E_LibraryManager.Services;
using E_LibraryManager.Utilties;
using E_LibraryManager.ViewModels;
using System.Transactions;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BooksService _booksService;
        private readonly EmailService _emailService;
        private readonly BookCategoryService _bookCategoryService;
        private readonly AuthorService _authorService;
        private readonly UserService _userService;
        private readonly BookTransactionsService _bookTransactionsService;
        private readonly BookReserveService _bookReserveServiceService;

        public BooksController(BooksService booksService, EmailService emailService, BookCategoryService bookCategoryService, AuthorService authorService, UserService userService, BookReserveService bookReserveService, BookTransactionsService bookTransactionsService)
        {
            _booksService = booksService;
            _emailService = emailService;
            _bookCategoryService = bookCategoryService;
            _authorService = authorService;
            _userService = userService;
            _bookTransactionsService = bookTransactionsService;
            _bookReserveServiceService = bookReserveService;
        }

        [HttpGet]
        public async Task<List<Book>> Get()
        {
            return await _booksService.GetBooksAsync();
        }

        [HttpGet("{bookId}")]
        public async Task<Book> Get(string bookId)
        {
            return await _booksService.GetBookAsync(bookId);
        }


        [HttpGet("adminIssuedBooks")]
        public async Task<List<BookRecord>> AdminIssuedBooks()
        {
            return await _booksService.AdminIssuedBooks();
        }

        [HttpGet("userIssuedBooks/{userId}")]
        public async Task<List<BookRecord>> UserIssuedBooks(string userId)
        {
            return await _booksService.UserIssuedBooks(userId);
        }


        [HttpPost]
        public async Task<IActionResult> Post(BookVM model)
        {
            var book = new Book()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = model.Title,
                ISBN = string.IsNullOrEmpty(model.ISBN) ? Guid.NewGuid().ToString() : model.ISBN,
                Description = model.Description,
                Author = model.Author,
                PublishedYear = model.PublishedYear,
                Category = model.Category,
                Image = "https://images.pond5.com/animation-book-opening-footage-079060528_iconl.jpeg",
                Status = BookStatus.Available.ToString(),
                isAvailable = true,
                //bookTransactions = new List<BookTransaction>(),
                BookId = string.IsNullOrEmpty(model.BookId) ? Guid.NewGuid().ToString() : model.BookId
            };

            await _booksService.CreateAsync(book);

            return CreatedAtAction(nameof(Get), new { id = book.Title }, book);
        }

        [HttpPost("ReserveBook")]

        public async Task<IActionResult> ReserveBook(BookRequests bookRequests)
        {
            var bookRecord = await _booksService.UserIssuedBooks(bookRequests.UserId);
            bookRecord = bookRecord.Where(x => x.bookTransactions.IsActive).ToList();
            if (bookRecord.Count >= 3)
            {
                return BadRequest("LIMIT_EXCEEDED");
            }

            var book = await _booksService.GetBookAsync(bookRequests.BookId);
            var user = await _userService.GetAsync(bookRequests.UserId);

            if (book == null || user == null)
            {
                return BadRequest();
            }
            else
            {
                var reservations = await _bookReserveServiceService.GetBookReservationsAsync();
                if (reservations.Where(x => x.BookId == bookRequests.BookId).Any())
                {
                    return Conflict("Already Reserved");
                }
                else if (bookRecord.Where(x => x.BookId == bookRequests.BookId).Any())
                {
                    return Conflict("Already Issued");
                }
                var reservation = new BookReservations();
                reservation.Id = ObjectId.GenerateNewId().ToString();
                reservation.UserId = bookRequests.UserId;
                reservation.BookId = bookRequests.BookId;
                reservation.ReservedTime = DateTime.Now;
                await _bookReserveServiceService.CreateBookReservationsAsync(reservation);

                var emailBody = PrepareEmailBody.GetEmailBody(new MailBodyProperties()
                {
                    BookId = book.BookId,
                    Status = BookStatus.Requested.ToString(),
                    UserId = user.UserId,
                    ISBN = book.ISBN,
                    Title = book.Title,
                    Description = book.Description,
                });
                _emailService.SendEmail(new Message(new string[] { user.Email }, "Book Requested", emailBody, new string[] { }, new string[] { }, true));

                return Ok(true);
            }
        }

        [HttpPost("Checkin")]
        public async Task<IActionResult> Checkin(TransactionVM transaction)
        {
            var bookRecord = await _booksService.UserIssuedBooks(transaction.UserId);
            bookRecord = bookRecord.Where(x => x.bookTransactions.IsActive).ToList();
            if (bookRecord.Count >= 3)
            {
                return BadRequest("LIMIT_EXCEEDED");
            }

            var book = await _booksService.GetBookAsync(transaction.BookId);
            var user = await _userService.GetAsync(transaction.UserId);

            if (book == null || user == null)
            {
                return BadRequest();
            }
            else
            {
                var bookTransaction = new BookTransaction()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.UserId,
                    BookId = transaction.BookId,
                    TransactionDate = DateTime.Now,
                    CheckInDateTime = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(15),
                    Status = BookStatus.Issued.ToString(),
                    IsActive = true,
                    TransactionId = Guid.NewGuid().ToString()
                };
                await _bookTransactionsService.CreateTransactionAsync(bookTransaction);
                // book.bookTransactions.Add(bookTransaction);
                book.Status = BookStatus.NotAvailable.ToString();
                book.isAvailable = false;
                await _booksService.UpdateAsync(book.Id, book);

                var emailBody = PrepareEmailBody.GetEmailBody(new MailBodyProperties()
                {
                    BookId = transaction.BookId,
                    Status = BookStatus.Issued.ToString(),
                    UserId = user.UserId,
                    DueDate = bookTransaction.DueDate,
                    DateTaken = bookTransaction.CheckInDateTime,
                    ISBN = book.ISBN,
                    Title = book.Title,
                    Description = book.Description,
                });
                _emailService.SendEmail(new Message(new string[] { user.Email }, "Book Issued", emailBody, new string[] { }, new string[] { }, true));

                return Ok(true);
            }
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string id)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not selected or empty.");
            }

            var book = await _booksService.GetBookAsync(id);

            if (book == null)
            {
                return BadRequest();
            }
            else
            {

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] fileData = memoryStream.ToArray();
                    book.FileData = fileData;
                    book.FileType = file.ContentType;
                    var res = await _booksService.UpdateAsync(book.Id, book);
                }
            }

            return Ok("File uploaded successfully.");
        }

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout(TransactionVM transaction)
        {
            var bookRecord = await _booksService.GetTransaction(transaction.TransactionId);
            var book = await _booksService.GetBookAsync(transaction.BookId);
            var user = await _userService.GetAsync(transaction.UserId);

            if (book == null || user == null || bookRecord == null)
            {
                return BadRequest();
            }
            else
            {
                book.isAvailable = true;
                book.Status = BookStatus.Available.ToString();
                var res = await _booksService.UpdateAsync(book.Id, book);
                await _booksService.UpdateTransaction(transaction, isRenew: false);


                bookRecord = await _booksService.GetTransaction(transaction.TransactionId);

                var emailBody = PrepareEmailBody.GetEmailBody(new MailBodyProperties()
                {
                    BookId = transaction.BookId,
                    Status = BookStatus.Returned.ToString(),
                    UserId = user.UserId,
                    DueDate = bookRecord.bookTransactions.DueDate,
                    DateTaken = bookRecord.bookTransactions.CheckInDateTime,
                    ISBN = book.ISBN,
                    Title = book.Title,
                    Description = book.Description,
                });
                _emailService.SendEmail(new Message(new string[] { user.Email }, "Book Returned", emailBody, new string[] { }, new string[] { }, true));

                //var reservations = _bookReservationsCollection.Find(x => x.BookId == item.BookId).ToList().OrderByDescending(x => x.ReservedTime).ToList();
                reserveBookToUser(transaction);
;
                return Ok(true);
            }
        }
        private async void reserveBookToUser(TransactionVM transaction)
        {
            var reservations = _bookReserveServiceService.GetBookReservationsByBookIdWithTime(transaction.BookId);
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
                        await _bookReserveServiceService.RemoveAsync(reservation.Id);
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
        [HttpPost("Renew")]
        public async Task<IActionResult> Renew(TransactionVM transaction)
        {
            var bookRecord = await _booksService.GetTransaction(transaction.TransactionId);
            var book = await _booksService.GetBookAsync(transaction.BookId);
            var user = await _userService.GetAsync(transaction.UserId);

            if (book == null || user == null || bookRecord == null)
            {
                return BadRequest();
            }
            else if (bookRecord.bookTransactions.RenewalCount >= 3)
            {
                return BadRequest("LIMIT_EXCEEDED");
            }
            else
            {
                await _booksService.CheckoutAsync(transaction, bookRecord, book, user,true);
                return Ok(true);
            }
        }

        [HttpGet("dashboard")]
        public async Task<DashboardModel> DashboardData()
        {
            return new DashboardModel()
            {
                TotalPublishers = await _authorService.GetTotalAuthorCount(),
                TotalBooks = await _booksService.GetTotalBookCount(),
                TotalGenres = await _bookCategoryService.GetTotalCategoryCount(),
                TotalUsers = await _userService.GetTotalUserCount()
            };
        }

    }
}
