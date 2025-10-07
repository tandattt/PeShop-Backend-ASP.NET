namespace PeShop.Interfaces
{
    public interface IApiHelper
    {
        /// <summary>
        /// Gọi API GET
        /// </summary>
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Gọi API POST
        /// </summary>
        Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Gọi API PUT
        /// </summary>
        Task<T?> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Gọi API DELETE
        /// </summary>
        Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Gọi API với form data
        /// </summary>
        Task<T?> PostFormAsync<T>(string url, Dictionary<string, string> formData, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Lấy raw response để debug
        /// </summary>
        Task<string> GetRawResponseAsync(string url, object? data = null, Dictionary<string, string>? headers = null);
    }
}
