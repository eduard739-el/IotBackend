using IotCloudServices.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IotCloudServices.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model.Username == "testuser" && model.Password == "testpassword") // Sample validation
            {
                var token = JwtTokenHelper.GenerateToken(model.Username, _configuration);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid username or password.");
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
