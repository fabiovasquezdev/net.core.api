using System.Text.Json;
using StackExchange.Redis;

namespace Repository.Redis
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
    }
} 