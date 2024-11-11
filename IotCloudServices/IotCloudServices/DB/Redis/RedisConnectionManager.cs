using IotCloudServices.Common.JWT;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

namespace IotCloudServices.Services.Authentication.DB.Redis
{

    public class RedisConnectionManager : IDisposable
    {
        private ConnectionMultiplexer _connection;
        private readonly RedisSettings _redisSettings;
        
        public RedisConnectionManager( IOptions<RedisSettings> redisSettings)
        {
            _redisSettings = redisSettings.Value;
            _connection = ConnectionMultiplexer.Connect(_redisSettings.ConnectionString);
        }

        // Get a connection to Redis
        public ConnectionMultiplexer GetConnection()
        {
            return _connection;
        }

        // Get database from Redis
        public StackExchange.Redis.IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }

        // Example method to check if Redis is alive
        public bool IsRedisAlive()
        {
            try
            {
                var db = GetDatabase();
                return db.Ping() != TimeSpan.Zero;
            }
            catch
            {
                return false;
            }
        }

        // Dispose method to clean up connection
        public void Dispose()
        {
            if (_connection != null && _connection.IsConnected)
            {
                _connection.Dispose();
            }
        }
    }

}
