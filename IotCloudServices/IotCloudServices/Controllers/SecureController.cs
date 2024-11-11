using IotCloudServices.Common.JWT;
using IotCloudServices.Services.Authentication.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IotCloudServices.Authentication.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {

        CommonTokenService _tokenVerifer;
        private readonly MyAppDbContext _context;

        public SecureController(CommonTokenService tokenVerifer, MyAppDbContext context)
        {
            _tokenVerifer = tokenVerifer;
            _context = context;
        }

        [HttpGet("Secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            return Ok("This is secure data.");

            // Extract the token from the Authorization header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            string errMsg;
            if (_tokenVerifer.VerifyToken(token, out errMsg))
            {
                return Ok("yeayy!");
            }
            else
            {
                return Unauthorized(errMsg);
            }


            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized("Token is missing.");
            }

#if false
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            try
            {
                // Validate the token
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _validIssuer,
                    ValidateAudience = true,
                    ValidAudience = _validAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Optional: Adjust for clock skew
                }, out SecurityToken validatedToken);

                // If the token is valid, you can access claims
                var jwtToken = (JwtSecurityToken)validatedToken;
                //var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                //var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Assuming you set this in the token
                var userName = User.Identity.Name; // This will give you the username if set



                // Return secured data
                var secureData = new
                {
                    Message = "This is secured data",
                    UserId = userId,
                    UserName = userName
                };

                return Ok(secureData);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Unauthorized("Invalid token signature.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
#endif
        }
    }
}
