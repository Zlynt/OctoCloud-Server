using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
// Database
using DatabaseClass =  OctoCloud.Server.Data.Database;
// Models
using UserModel = OctoCloud.Server.Models.User;

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

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest){
            try
            {
                UserModel user = new UserModel(loginRequest.Username);

                if(user.VerifyPassword(loginRequest.Password)){
                    // Assign user to the current session
                    //HttpContext.Session.SetString("Username", user.Username);

                    string userJson = JsonSerializer.Serialize(user);
                    HttpContext.Session.SetString("User", userJson);
                    List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username)};
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "UserSessionAuth");
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties { IsPersistent = true };

                    HttpContext.SignInAsync("UserSessionAuth", new ClaimsPrincipal(claimsIdentity), authenticationProperties).Wait();

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

        [HttpPost("Logout")] 
        public IActionResult Logout() { 
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync("UserSessionAuth").Wait();
            return Ok(new { Message = "Logout successful" });
        }

        [HttpGet("Current-User")] 
        public IActionResult GetCurrentUser() { 
            // Retrieve user object from session
            string userJson = HttpContext.Session.GetString("User"); 
            if (string.IsNullOrEmpty(userJson)) { 
                return Unauthorized(new { Message = "No user is currently logged in" });
            }
            UserModel user = JsonSerializer.Deserialize<UserModel>(userJson);

            return Ok(new { Username = user.Username });
        }
    }
}
