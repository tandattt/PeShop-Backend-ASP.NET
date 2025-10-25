using System.Text;
using System.Text.Json;
using PeShop.Interfaces;

namespace PeShop.Helpers
{
    public class ApiHelper : IApiHelper
    {
        private readonly HttpClient _httpClient;

        public ApiHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gọi API GET
        /// </summary>
        public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi GET API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gọi API POST
        /// </summary>
        public async Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                // Console.WriteLine("2"+response);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error {response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("3"+JsonSerializer.Deserialize<T>(content));
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi POST API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gọi API PUT
        /// </summary>
        public async Task<T?> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi PUT API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gọi API DELETE
        /// </summary>
        public async Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi DELETE API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gọi API với form data
        /// </summary>
        public async Task<T?> PostFormAsync<T>(string url, Dictionary<string, string> formData, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                var formContent = new FormUrlEncodedContent(formData);
                request.Content = formContent;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi POST Form API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gọi API với multipart form data (cho file upload)
        /// </summary>
        public async Task<T?> PostMultipartFormAsync<T>(string url, MultipartFormDataContent formData, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                request.Content = formData;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi POST Multipart Form API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy raw response để debug
        /// </summary>
        public async Task<string> GetRawResponseAsync(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
