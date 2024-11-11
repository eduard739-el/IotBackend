using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IotCloudServices.Common.JWT
{
    public class CommonTokenService
    {

        //private readonly string _secretKey; // Your secret key
        //private readonly string _validIssuer; // Your valid issuer
        //private readonly string _validAudience; // Your valid audience
        private readonly JwtSettings _jwtSettings;
        public CommonTokenService(IOptions<JwtSettings> jwtSettings) 
        {
            _jwtSettings = jwtSettings.Value;
        }


        public bool VerifyToken(string token, out string errorMsg)
        {
            
            if (string.IsNullOrWhiteSpace(token))
            {
                errorMsg = "Token is missing.";
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);


            try
            {
                // Validate the token
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Optional: Adjust for clock skew
                }, out SecurityToken validatedToken);

                errorMsg = string.Empty;
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                errorMsg = "Token has expired.";
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                errorMsg = "Invalid token signature.";
            }
            catch (Exception ex)
            {
                errorMsg = $"Token validation failed: {ex.Message}";
            }

            return false;
        }
    }
}

