namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
public class ShopService : IShopService
{
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;
    public ShopService(IShopRepository shopRepository, IProductRepository productRepository)
    {
        _shopRepository = shopRepository;
        _productRepository = productRepository;
    }
    public async Task<ShopDto> GetShopDetailAsync(string shopId)
    {
        var shop = await _shopRepository.GetShopByIdAsync(shopId);
        if (shop == null)
        {
            return new ShopDto();
        }
        return new ShopDto{
            Id = shop.Id,
            Name = shop.Name ?? string.Empty,
            Address = shop.FullNewAddress ?? string.Empty,
            Description = shop.Description ?? string.Empty,
            Logo = shop.LogoUrl ?? string.Empty,
            NewProviceId = shop.NewProviceId ?? string.Empty,
            ProductCount = shop.PrdCount ?? 0,
            FollowersCount = shop.FollowersCount ?? 0
        };
    }
}