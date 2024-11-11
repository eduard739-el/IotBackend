using IotCloudServices.Authentication.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Authentication.Models;
using IotCloudServices.Common.JWT;
using IotCloudServices.Services.Authentication.DB;
using IotCloudServices.Services.Authentication.Models;
using Microsoft.EntityFrameworkCore;
using IotCloudServices.Common.Utils.Passwords;
using IotCloudServices.Services.Authentication.Utils;
using System.Net.Sockets;
using IotCloudServices.Services.Authentication.Migrations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace IotCloudServices.Authentication.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly CommonTokenService _commonTokenService;
        private readonly MyAppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly RefreshTokenSessionManager _refreshTokenSessionManager;
        

        public LoginController(TokenService tokenService, CommonTokenService commonTokenService,
            MyAppDbContext context, PasswordService passwordService,RefreshTokenSessionManager refreshTokenSessionManager)
        {
            _tokenService = tokenService;
            _commonTokenService = commonTokenService;
            _context = context;
            _passwordService = passwordService;
            _refreshTokenSessionManager = refreshTokenSessionManager;

            AuthUser test = new AuthUser();
            test.UserName = "admin";
            test.PasswordHash = _passwordService .HashPassword("admin");
            AddUserIfNotExistAsync(test,_context).Wait();
        }

        static public async Task AddUserIfNotExistAsync(AuthUser newUser,MyAppDbContext _context)
        {
            // Check if a user with the same username already exists
            var existingUser = await _context.MyAuthUsers
                                              .FirstOrDefaultAsync(u => u.UserName == newUser.UserName);

            // If the user doesn't exist, add them
            if (existingUser == null)
            {
                await _context.MyAuthUsers.AddAsync(newUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Optionally handle the case where the user already exists
                // For example, return a message or throw an exception
                //  throw new Exception("User already exists.");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate user credentials (this would typically involve checking a database)
            var (isSuccess, authUser) = await IsValidUser(request.Username, request.Password);
            
            if (isSuccess)
            {
                //generate tokens
                var (accessToken, refreshToken,tokenLifetime) = _tokenService.GenerateTokens(authUser!.Id, authUser.UserName);



                //store refresh token
                await AddRefreshToken(refreshToken, authUser, tokenLifetime);

                // Return the tokens
                return Ok(new
                {
                    accessToken,
                    refreshToken
                });
            }

            return Unauthorized("Invalid username or password.");
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            // Validate the refresh token (check against your storage, e.g., database)
            var authUser = ValidateRefreshToken(request.RefreshToken); 

            if (authUser == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Generate new tokens
            var newTokens = _tokenService.GenerateTokens(authUser.Id, authUser.UserName);

            //remove old token and add new
            AddRefreshToken(newTokens.refreshToken,authUser, newTokens.tokenLifetime,request.RefreshToken).Wait();
            
            //TODO - invalidate old access token?
               
            return Ok(new
            {
                accessToken = newTokens.accessToken,
                refreshToken = newTokens.refreshToken 
            });
        }

        private AuthUser? ValidateRefreshToken(string refreshToken)
        {

            _refreshTokenSessionManager.TryGetUser(refreshToken, out var user);

            // Implement logic to validate the refresh token (check against your storage)
            // If valid, return the associated user ID; otherwise, return null.
            return user; // Example placeholder
        }

        private async Task<(bool isSuccess, AuthUser? user)> IsValidUser(string username, string password)
        {

            if(_context.MyAuthUsers == null)
                return (false, null);

            DbSet<AuthUser> myAuthUsers = _context!.MyAuthUsers;
            var existingUser = await myAuthUsers!
                                              .FirstOrDefaultAsync(u => u.UserName == username);

            if(existingUser == null) 
            {
                return (false, null);
            }

            if(!_passwordService.VerifyPassword(existingUser.PasswordHash, password))
                return (false, null);

            return (true, existingUser);
        }

        async private Task AddRefreshToken(string refreshToken,AuthUser authUser, TimeSpan tokenLifetime,string? oldTokenToRemove = null)
        {
            //store refresh token
            _refreshTokenSessionManager.AddToken(refreshToken, authUser, tokenLifetime);
            if(oldTokenToRemove != null) 
                _refreshTokenSessionManager.RemoveToken(oldTokenToRemove);


        }
    }

    

    

}
