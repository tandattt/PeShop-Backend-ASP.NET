namespace PeShop.Interfaces
{
    public interface IRedisUtil
    {
        // String operations
        Task<string?> GetAsync(string key);
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}