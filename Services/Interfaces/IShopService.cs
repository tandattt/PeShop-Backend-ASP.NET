namespace PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
public interface IShopService
{
    Task<ShopDto> GetShopDetailAsync(string shopId);
}