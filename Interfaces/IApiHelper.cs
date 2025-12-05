namespace PeShop.Interfaces
{
    public interface IApiHelper
    {
        /// <summary>
        /// Gọi API GET
        /// </summary>
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
        /// <summary>
        /// Gọi API GET raw
        /// </summary>
        Task<T?> GetRawAsync<T>(string url,object? data = null, Dictionary<string, string>? headers = null);
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
        /// Upload 1 file đơn giản
        /// </summary>
        Task<T?> PostFileAsync<T>(string url, IFormFile file, Dictionary<string, string>? formData = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Gọi API với multipart form data (cho file upload)
        /// </summary>
        Task<T?> PostMultipartFormAsync<T>(string url, MultipartFormDataContent formData, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Lấy raw response để debug
        /// </summary>
        Task<string> GetRawResponseAsync(string url, object? data = null, Dictionary<string, string>? headers = null);
    }
}
