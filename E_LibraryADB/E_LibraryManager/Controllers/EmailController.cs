using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_LibraryManager.Services;
using E_LibraryManager.ViewModels;

namespace E_LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService emailService;

        public EmailController(EmailService emailService)
        {
            this.emailService = emailService;
        }


        [HttpPost("send")]
        public IActionResult SendEmail(Email email )
        {
            emailService.SendEmailPHPProject(email);
            return Ok();
        }

    }
}
