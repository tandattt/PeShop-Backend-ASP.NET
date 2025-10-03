namespace PeShop.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(string userId);
        // Task<CartResponse> AddCartAsync(CartRequest request);
        // Task<CartResponse> UpdateCartAsync(CartRequest request);
        // Task<CartResponse> DeleteCartAsync(string userId);
    }
}