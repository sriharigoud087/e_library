using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using E_LibraryManager.Models;
using E_LibraryManager.Services;
using E_LibraryManager.Utilties;
using E_LibraryManager.ViewModels;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        public UserController(UserService userService,TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }
        
        [HttpGet]
        public async Task<List<User>> Get()
        {
            return await _userService.GetAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Getuser(string id)
        {
            var user = await _userService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Post(LoginVM loginVM)
        {
            var user = await _tokenService.UserLogin(loginVM);

            if (user is null)
            {
                return NotFound("Invalid Username/password");
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult> Post(UserVM model)
        {
            User user = new User();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Password = model.Password;
            user.Address = model.Address;
            user.Phone = model.Phone;
            user.City = model.City;
            user.Role = "User";
            user.ZipCode=model.ZipCode;
            user.UserId = Util.GetRandomNumber();
            user.Id = ObjectId.GenerateNewId().ToString();
            await _userService.CreateAsync(user);

            return CreatedAtAction(nameof(Getuser), new { id = user.UserId }, user);
        }
      
    }
}
