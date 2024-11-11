using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotCloudServices.Common.JWT;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IotCloudServices.Common.JWT
{

    public class JwtTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CommonTokenService _commonTokenService;

        public JwtTokenValidationMiddleware(RequestDelegate next, CommonTokenService commonTokenService)
        {
            _next = next;
            _commonTokenService = commonTokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // Check if the route has the [Authorize] attribute
            var endpoint = context.GetEndpoint();
            if (endpoint == null || !endpoint.Metadata.OfType<AuthorizeAttribute>().Any())
            {
                // Skip validation for endpoints without [Authorize] attribute
                await _next(context);
                return;
            }


            // Extract the token from the Authorization header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                // If no token is found, return 401 Unauthorized
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is missing.");
                return;
            }

            // Verify the token using CommonTokenService
            var isValid = _commonTokenService.VerifyToken(token, out string errorMsg);

            if (!isValid)
            {
                // If token is invalid, return 401 Unauthorized with the error message
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(errorMsg);
                return;
            }

            // Optionally, you can add claims to the context if needed
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            context.User = new ClaimsPrincipal(claimsIdentity);

            // Pass control to the next middleware in the pipeline
            await _next(context);
        }
    }

}
