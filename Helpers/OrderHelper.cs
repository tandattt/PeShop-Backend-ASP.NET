using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Shared;
using PeShop.Interfaces;
namespace PeShop.Helpers;

public class OrderHelper : IOrderHelper
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    
    public OrderHelper(IVoucherRepository voucherRepository, IProductRepository productRepository, IShopRepository shopRepository)
    {
        _voucherRepository = voucherRepository;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
    }
    public async Task<decimal> CalculateOrderTotalAsync(List<OrderRequest> items)
    {
        decimal total = 0;

        foreach (var item in items)
        {
            decimal price = 0;

            if (!string.IsNullOrEmpty(item.VariantId))
            {
                // Lấy giá từ variant
                var variant = await _voucherRepository.GetVariantByIdAsync(item.VariantId);
                if (variant != null)
                {
                    price = variant.Price ?? 0;
                }
            }
            else
            {
                // Lấy giá từ product
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    price = product.Price ?? 0;
                }
            }

            total += price * item.Quantity;
        }

        return total;
    }
    
    public async Task<List<ItemShop>> GroupItemsByShopAsync(List<OrderRequest> items)
    {
        var shopGroups = new Dictionary<string, List<OrderRequest>>();
        
        foreach (var item in items)
        {
            string shopId = item.ShopId;
            
            if (!shopGroups.ContainsKey(shopId))
            {
                shopGroups[shopId] = new List<OrderRequest>();
            }
            
            shopGroups[shopId].Add(item);
        }
        
        var itemShops = new List<ItemShop>();
        
        foreach (var shopGroup in shopGroups)
        {
            decimal shopTotal = 0;
            
            foreach (var product in shopGroup.Value)
            {
                decimal price = await GetProductPriceAsync(product);
                shopTotal += price * product.Quantity;
            }
            
            // Lấy thông tin shop
            var shop = await _shopRepository.GetShopByIdAsync(shopGroup.Key);
            
            itemShops.Add(new ItemShop
            {
                ShopId = shopGroup.Key,
                ShopName = shop?.Name ?? "Unknown Shop",
                ShopLogoUrl = shop?.LogoUrl,
                Products = shopGroup.Value,
                PriceOriginal = shopTotal
            });
        }
        
        return itemShops;
    }
    
    private async Task<decimal> GetProductPriceAsync(OrderRequest item)
    {
        decimal price = 0;
        
        if (!string.IsNullOrEmpty(item.VariantId))
        {
            // Lấy giá từ variant
            var variant = await _voucherRepository.GetVariantByIdAsync(item.VariantId);
            if (variant != null)
            {
                price = variant.Price ?? 0;
            }
        }
        else
        {
            // Lấy giá từ product
            var product = await _productRepository.GetProductByIdAsync(item.ProductId);
            if (product != null)
            {
                price = product.Price ?? 0;
            }
        }
        
        return price;
    }
}