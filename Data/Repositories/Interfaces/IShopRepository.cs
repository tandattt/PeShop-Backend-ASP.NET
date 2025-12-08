using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IShopRepository
{
    Task<Shop> GetAddressShopById(string id);
    Task<List<Shop>> SearchShopsAsync(string keyword, int skip = 0, int take = 20);
    Task<int> GetSearchShopsCountAsync(string keyword);
    Task<Shop?> GetShopByIdAsync(string shopId);
    Task<Shop?> GetShopByUserIdAsync(string userId);
}
