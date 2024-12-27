using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using OctoCloud.Server.Database;

namespace OctoCloud.Server.Controllers
{
    public class LoginRequest { 
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {

        public AccountController() : base() { }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest){
            try
            {
                User user = new User(loginRequest.Username);

                if(user.VerifyPassword(loginRequest.Password)){
                    // Assign user to the current session
                    HttpContext.Session.SetString("Username", user.Username);

                    return Ok(new { Message = "Login successful" } );
                } else {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }
            } catch(Exception ex) {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new { Message = "An error occurred", Details = ex.Message }
                );
            }
        }
    }
}
