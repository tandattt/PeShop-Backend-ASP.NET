using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartResponse>> GetCartAsync(string userId);
        Task<Dictionary<string, int>> AddCartAsync(CartRequest request, string userId);
        Task<CartResponse> UpdateCartAsync(string cartId, int quantity, string userId);
        Task<string> DeleteCartAsync(string cartId, string userId);
        Task<string> ClearCartAsync(string userId);
        Task<Dictionary<string, int>> GetCartCountAsync(string userId);
    }
}