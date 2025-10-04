using PeShop.Dtos.Requests;
using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<List<Cart>?> GetCartAsync(string userId);
        Task<Cart?> GetCartByIdAsync(string cartId);
        Task<Cart?> AddCartAsync(Cart cart, string userId);
        Task<Cart?> UpdateCartAsync(Cart cart);
        Task<Cart?> DeleteCartAsync(string cartId);
        Task<bool> ClearCartAsync(string userId);
    }
}