using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EventSphereAPI.Controllers
{
    [ApiController]
    [Route("api/[action]")]
    public class AuthController : ControllerBase
    {
        // Mock user data for demonstration (replace with a real database)
        private readonly Dictionary<string, string> _mockUsers = new Dictionary<string, string>
        {
            { "user1", "password1" },
            { "admin", "admin123" }
        };

        [HttpPost]
        [ActionName("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new { responseMessage = "Username and Password are required." });
            }

            if (_mockUsers.TryGetValue(loginRequest.Username, out var storedPassword) &&
                storedPassword == loginRequest.Password)
            {
                // Simulating session storage using cookies or tokens
                HttpContext.Session.SetString("UserSession", loginRequest.Username);

                return Ok(new
                {
                    result = new
                    {
                        responseMessage = "Logined Successfully",
                        user = loginRequest.Username
                    }
                });
            }

            return Unauthorized(new { responseMessage = "Invalid credentials." });
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session
            return Ok(new { responseMessage = "Logged out successfully." });
        }
    }

    // DTO for login request
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
