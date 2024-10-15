using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using E_LibraryManager.Models;
using E_LibraryManager.Services;
using E_LibraryManager.Utilties;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly AuthorService _authorService;
        public AuthorController(AuthorService booksService)
        {
            _authorService = booksService;
        }

        [HttpGet]
        public async Task<List<Author>> Get()
        {
            return await _authorService.GetAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> Get(int id)
        {
            var author = await _authorService.GetAsync(id);

            if (author is null)
            {
                return NotFound();
            }

            return author;
        }

        [HttpPost]
        public async Task<ActionResult<Author>> Post(AuthorVM model)
        {
            Author author = new Author();
            author.Name = model.Name;
            author.AuthorId = Convert.ToInt32(Util.GetRandomNumber());
            author.Id = ObjectId.GenerateNewId().ToString();

            await _authorService.CreateAsync(author);

            return CreatedAtAction(nameof(Get), new { id = author.AuthorId }, author);
        }
    }
}
