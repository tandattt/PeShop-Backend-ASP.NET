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
    private readonly IFlashSaleRepository _flashSaleRepository;
    
    public OrderHelper(IVoucherRepository voucherRepository, IProductRepository productRepository, IShopRepository shopRepository, IFlashSaleRepository flashSaleRepository)
    {
        _voucherRepository = voucherRepository;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
        _flashSaleRepository = flashSaleRepository;
    }
    public async Task<decimal> CalculateOrderTotalAsync(List<OrderRequest> items)
    {
        decimal total = 0;

        foreach (var item in items)
        {
            decimal price = 0;

            if (item.VariantId != null)
            {
                // Lấy giá từ variant
                var variant = await _voucherRepository.GetVariantByIdAsync(item.VariantId ?? 0);
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
        
        // Batch check FlashSale cho tất cả products
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var flashSaleStatuses = await _flashSaleRepository.HasFlashSalesForProductsAsync(productIds);
        var flashSaleDiscounts = await _flashSaleRepository.GetFlashSaleDiscountsForProductsAsync(productIds);
        var activeFlashSales = await _flashSaleRepository.GetActiveFlashSaleProductsAsync(productIds);
        
        var itemShops = new List<ItemShop>();
        
        foreach (var shopGroup in shopGroups)
        {
            decimal shopTotal = 0;
            decimal shopFlashSaleDiscount = 0;
            var orderCode = (1000000 + new Random().Next(1000000));
            
            foreach (var product in shopGroup.Value)
            {
                productDto productDto = await GetProductPriceAsync(product);
                product.CategoryId = productDto.CategoryId;
                
                // Check và apply FlashSale
                if (flashSaleStatuses.ContainsKey(product.ProductId) && flashSaleStatuses[product.ProductId])
                {
                    product.FlashSalePercentDecrease = flashSaleDiscounts[product.ProductId];
                    
                    // Lấy FlashSaleProductId
                    if (activeFlashSales.TryGetValue(product.ProductId, out var flashSaleProduct))
                    {
                        product.FlashSaleProductId = flashSaleProduct.Id;
                    }
                    
                    // Tính giá FlashSale
                    var flashSalePrice = productDto.Price * (100 - flashSaleDiscounts[product.ProductId]) / 100m;
                    product.FlashSalePrice = flashSalePrice;
                    
                    // Dùng giá FlashSale
                    product.PriceOriginal = flashSalePrice * product.Quantity;
                    
                    // Tính discount
                    var discount = (productDto.Price - flashSalePrice) * product.Quantity;
                    shopFlashSaleDiscount += discount;
                }
                else
                {
                    product.PriceOriginal = productDto.Price * product.Quantity;
                }
                
                shopTotal += product.PriceOriginal;
            }
            
            // Lấy thông tin shop
            var shop = await _shopRepository.GetShopByIdAsync(shopGroup.Key);
            
            itemShops.Add(new ItemShop
            {
                ShopId = shopGroup.Key,
                ShopName = shop?.Name ?? "Unknown Shop",
                ShopLogoUrl = shop?.LogoUrl,
                Products = shopGroup.Value,
                OrderCode = orderCode.ToString(),
                PriceOriginal = shopTotal,
                FlashSaleDiscount = shopFlashSaleDiscount
            });
        }
        
        return itemShops;
    }
    private class productDto{
        public decimal Price { get; set; } = 0;
        public string CategoryId { get; set; } = string.Empty;
    }
    private async Task<productDto> GetProductPriceAsync(OrderRequest item)
    {
        // decimal price = 0;
        productDto productDto = new productDto();
        if (item.VariantId != null)
        {
            // Lấy giá từ variant
            var variant = await _voucherRepository.GetVariantByIdAsync(item.VariantId ?? 0);
            if (variant != null)
            {
                productDto.Price = variant.Price ?? 0;
                productDto.CategoryId = variant.Product.CategoryId ?? string.Empty;
            }
        }
        else
        {
            // Lấy giá từ product
            var product = await _productRepository.GetProductByIdAsync(item.ProductId);
            if (product != null)
            {
                productDto.Price = product.Price ?? 0;
                productDto.CategoryId = product.CategoryId ?? string.Empty;
            }
        }
        
        return productDto;
    }
}