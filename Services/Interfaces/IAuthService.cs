using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;    
namespace PeShop.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<StatusResponse> RegisterAsync(RegisterRequest request);
        // Task<string?> GetUserIdFromTokenAsync(string token);
        // Task<List<string>> GetRolesFromTokenAsync(string token);
        // Task<string?> GetShopIdFromTokenAsync(string token);
    }
}
