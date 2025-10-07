using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IShopRepository
{
    Task<Shop> GetAddressShopById(string id);
}
