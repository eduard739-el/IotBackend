using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IotCloudServices.Authentication.Utils
{
    public class TokenService
    {
        private readonly string _secretKey;
        private readonly string _validIssuer;
        private readonly string _validAudience;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["JwtSettings:SecretKey"];
            _validIssuer = configuration["JwtSettings:Issuer"];
            _validAudience = configuration["JwtSettings:Audience"];
        }

        public (string accessToken, string refreshToken, TimeSpan tokenLifetime) GenerateTokens(UserDbIndexType userId, string userName)
        {
         

            var tokenLifetime = TimeSpan.FromMinutes(30);
            // Create claims
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName)
            // Add other claims as necessary
            };

            // Create the access token
            var accessToken = CreateToken(claims, tokenLifetime); // Short-lived

            // Create the refresh token
            var refreshToken = Guid.NewGuid().ToString(); // Simple example, replace with secure storage

            // Save the refresh token in a database or in-memory store, associated with the user

            return (accessToken, refreshToken, tokenLifetime);
        }

        private string CreateToken(IEnumerable<Claim> claims, TimeSpan expiration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.Now.Add(expiration),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
