using IotCloudServices.Services.Authentication.DB.Redis;
using IotCloudServices.Services.Authentication.DB.Redis.Models;
using IotCloudServices.Services.Authentication.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace IotCloudServices.Services.Authentication.Utils
{
    //tmp solution , should be saved in radis or shared memory.
    public class RefreshTokenSessionManager : IDisposable
    {
        //TODO - save actual authUser or only id and refer to DB each refresh ? (user can be already deleted and refreshed - solve with event?)
        private readonly ConcurrentDictionary<string, (AuthUser User, DateTime Expiration)> _refreshTokens;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly RedisRefreshTokenManager _redisRefreshTokenManager;
        public RefreshTokenSessionManager(RedisRefreshTokenManager redisRefreshTokenManager)
        {
            _redisRefreshTokenManager = redisRefreshTokenManager;
            _refreshTokens = new ConcurrentDictionary<string, (AuthUser User, DateTime Expiration)>();
            _cancellationTokenSource = new CancellationTokenSource();

            // Start the cleanup task to remove expired tokens
            StartCleanupTask();
        }

        // Adds a refresh token with a custom expiration time
        public void AddToken(string refreshToken, AuthUser user, TimeSpan tokenLifetime)
        {
            var expirationTime = DateTime.UtcNow.Add(tokenLifetime);
            _refreshTokens[refreshToken] = (user, expirationTime);
            _redisRefreshTokenManager.SaveRefreshTokenAsync(refreshToken, user, DateTime.UtcNow.Add(tokenLifetime)).Wait();
        }

        // Removes a refresh token
        public bool RemoveToken(string refreshToken)
        {
            _redisRefreshTokenManager.RemoveRefreshTokenAsync(refreshToken).Wait();
            return _refreshTokens.TryRemove(refreshToken, out _);
        }

        // Attempts to get the AuthUser associated with a refresh token
        public bool TryGetUser(string refreshToken, out AuthUser? user)
        {
            if (_refreshTokens.TryGetValue(refreshToken, out var entry) && entry.Expiration > DateTime.UtcNow)
            {
                user = entry.User;
                return true;
            }

            var redisResult = _redisRefreshTokenManager.GetRefreshTokenAsync(refreshToken).Result;

            if (redisResult.User != null)
            {
                user = redisResult.User;
                return true;
            }

            user = null;
            return false;
        }

        // Task to periodically check and remove expired tokens
        private void StartCleanupTask()
        {
            _ = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    foreach (var token in _refreshTokens.Keys)
                    {
                        if (_refreshTokens.TryGetValue(token, out var entry) && entry.Expiration <= DateTime.UtcNow)
                        {
                            // Remove expired tokens
                            RemoveToken(token);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), _cancellationTokenSource.Token); // Adjust cleanup interval as needed
                }
            });
        }

        // Method to stop the cleanup task when the manager is disposed
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }

}
