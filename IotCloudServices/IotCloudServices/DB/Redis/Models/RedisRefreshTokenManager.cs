using IotCloudServices.Services.Authentication.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace IotCloudServices.Services.Authentication.DB.Redis.Models
{
    public class RedisRefreshTokenManager
    {
        private readonly RedisConnectionManager _connectionManager;
        private readonly IDatabase _database;

        public RedisRefreshTokenManager(RedisConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _database = _connectionManager.GetDatabase();
        }

        // Save a refresh token in a Redis Hash
        public async Task SaveRefreshTokenAsync(string tokenKey, AuthUser user, DateTime expiration)
        {
            var refreshToken = new
            {
                User = user,
                Expiration = expiration
            };

            // Convert the refresh token object to a JSON string
            var serializedToken = JsonConvert.SerializeObject(refreshToken);

            // Save the token under its unique tokenKey in a hash
            await _database.HashSetAsync("refresh_tokens", tokenKey, serializedToken);
        }

        // Retrieve a specific refresh token by its key from the Redis Hash
        public async Task<(AuthUser? User, DateTime Expiration)> GetRefreshTokenAsync(string tokenKey)
        {
            var serializedToken = await _database.HashGetAsync("refresh_tokens", tokenKey);

            if (serializedToken.IsNullOrEmpty)
            {
                return (null, DateTime.MinValue); // Token not found
            }

            // Deserialize the token from JSON
            var token = JsonConvert.DeserializeObject<dynamic>(serializedToken);
            var user = token.User.ToObject<AuthUser>();
            var expiration = token.Expiration.ToObject<DateTime>();

            return (user, expiration);
        }

        // Remove a specific refresh token by its key from the Redis Hash
        public async Task RemoveRefreshTokenAsync(string tokenKey)
        {
            await _database.HashDeleteAsync("refresh_tokens", tokenKey);
        }
    }

}
