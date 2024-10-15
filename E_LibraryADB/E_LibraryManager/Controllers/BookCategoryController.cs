using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using E_LibraryManager.Models;
using E_LibraryManager.Services;
using E_LibraryManager.Utilties;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookCategoryController : ControllerBase
    {
        private readonly BookCategoryService _bookCategoryService;
        public BookCategoryController(BookCategoryService bookCategoryService)
        {
            _bookCategoryService = bookCategoryService;
        }

        [HttpGet]
        public async Task<List<BookCategory>> Get()
        {
            return await _bookCategoryService.GetAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookCategory>> Get(int id)
        {
            var bookCategory = await _bookCategoryService.GetAsync(id);

            if (bookCategory is null)
            {
                return NotFound();
            }

            return bookCategory;
        }

        [HttpPost]
        public async Task<ActionResult<BookCategory>> Post(BookCategoryVM model)
        {
            BookCategory bookCategory = new BookCategory();
            bookCategory.CategoryName = model.CategoryName;
            bookCategory.CategoryId = Convert.ToInt32(Util.GetRandomNumber());
            bookCategory.Id = ObjectId.GenerateNewId().ToString();

            await _bookCategoryService.CreateAsync(bookCategory);

            return CreatedAtAction(nameof(Get), new { id = bookCategory.CategoryId }, bookCategory);
        }
    }

}
