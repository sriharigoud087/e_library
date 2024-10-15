using Microsoft.AspNetCore.Mvc;
using E_LibraryManager.Models;
using E_LibraryManager.Services;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly BooksService _booksService;
        private readonly BookCategoryService _bookCategoryService;
        private readonly AuthorService _authorService;
        private readonly UserService _userService;
        private readonly TransactionService _transactionService;

        public TransactionController(BooksService booksService, BookCategoryService bookCategoryService,
            AuthorService authorService, UserService userService,TransactionService transactionService)
        {
            _booksService = booksService;
            _bookCategoryService = bookCategoryService;
            _authorService = authorService;
            _userService = userService;
            _transactionService = transactionService;
        }
        [HttpGet]
        public async Task<List<BookTransaction>> Get()
        {
            return await _transactionService.GetAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookTransaction>> Get(string id)
        {
            var transaction = await _transactionService.GetAsync(id);

            if (transaction is null)
            {
                return NotFound();
            }

            return transaction;
        }
    }
}
