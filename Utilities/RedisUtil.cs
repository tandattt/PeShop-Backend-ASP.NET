using StackExchange.Redis;
using PeShop.Interfaces;
using System.Text.Json;
using PeShop.Setting;

namespace PeShop.Utilities
{
    public class RedisUtil : IRedisUtil
    {
        private readonly IDatabase _db;
        private readonly AppSetting _appSetting;

        public RedisUtil(IConnectionMultiplexer redis, AppSetting appSetting)
        {
            _db = redis.GetDatabase();
            _appSetting = appSetting;
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(_appSetting.NameProjectRedis + ":" + key);
            return value.HasValue ? value.ToString() : null;
        }
        public async Task<KeyValuePair<string?, TimeSpan?>> GetAsyncWithTtl(string key)
        {
            var value = await _db.StringGetAsync(_appSetting.NameProjectRedis + ":" + key);
            var ttl = await _db.KeyTimeToLiveAsync(_appSetting.NameProjectRedis + ":" + key);
            return new KeyValuePair<string?, TimeSpan?>(value.HasValue ? value.ToString() : null, ttl);
        }
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _db.StringGetAsync(_appSetting.NameProjectRedis + ":" + key);
            if (!value.HasValue) return null;
            
            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _db.StringSetAsync(_appSetting.NameProjectRedis + ":" + key, value, expiry);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(_appSetting.NameProjectRedis + ":" + key, json, expiry);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await _db.KeyDeleteAsync(_appSetting.NameProjectRedis + ":" + key);
        }
        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(_appSetting.NameProjectRedis + ":" + key);
        }
    }
}