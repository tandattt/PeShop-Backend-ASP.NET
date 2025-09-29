using StackExchange.Redis;
using PeShop.Interfaces;
using System.Text.Json;

namespace PeShop.Utilities
{
    public class RedisUtil : IRedisUtil
    {
        private readonly IDatabase _db;

        public RedisUtil(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return null;
            
            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _db.StringSetAsync(key, value, expiry);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(key, json, expiry);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }
    }
}