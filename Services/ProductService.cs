using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Interfaces;
using System.Text.Json;
using PeShop.Dtos.Responses;
namespace PeShop.Services;
using PeShop.Models.Entities;
public class ProductService : IProductService
{
    private readonly IRedisUtil _redisUtil;
    private readonly IProductRepository _productRepository;
    public ProductService(IProductRepository productRepository, IRedisUtil redisUtil)
    {
        _productRepository = productRepository;
        _redisUtil = redisUtil;
    }
    public async Task<List<ProductDto>> GetFirstListProductAsync(string userId)
    {
        var products = await GetListProductAsync();
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty
        }).ToList();
        var gotListProductId = productDtos.Select(p => p.Id).ToList();
        await _redisUtil.SetAsync("gotListProductId:" + userId, JsonSerializer.Serialize(gotListProductId), TimeSpan.FromMinutes(15));
        return productDtos;
    }
    private async Task<List<Product>> GetListProductAsync()
    {
        var count = await _productRepository.GetCountProductAsync();
        count = count < 15 ? count : count - 15;
        var random = new Random();
        var skip = random.Next(0, count);
        var take = 15;
        var ListProducts = await _productRepository.GetListProductAsync(skip, take);
        return ListProducts;
    }
    public async Task<List<ProductDto>> GetNextListProductAsync(string userId)
    {
        var gotListProductId = await _redisUtil.GetAsync<List<string>>("gotListProductId:" + userId);
        Console.WriteLine("gotListProductId: " + JsonSerializer.Serialize(gotListProductId));
        var ListProducts = await GetListProductAsync();
        var products = ListProducts.Where(p => !gotListProductId.Contains(p.Id)).ToList();
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty
        }).ToList();
        gotListProductId = gotListProductId.Concat(productDtos.Select(p => p.Id)).Distinct().ToList();
        await _redisUtil.SetAsync("gotListProductId:" + userId, JsonSerializer.Serialize(gotListProductId), TimeSpan.FromMinutes(15));
        return productDtos;
    }
    public async Task<ProductDetailResponse> GetProductDetailAsync(string productId)
    {
        var product = await _productRepository.GetProductByIdAsync(productId);
        return new ProductDetailResponse
        {
            BoughtCount = product.BoughtCount ?? 0,
            Description = product.Description ?? string.Empty,
            ImgMain = product.ImgMain ?? string.Empty,
            LikeCount = product.LikeCount ?? 0,
            ReviewCount = product.ReviewCount ?? 0,
            ReviewPoint = product.ReviewPoint ?? 0,
            Slug = product.Slug ?? string.Empty,
            ViewCount = product.ViewCount ?? 0,
            ShopId = product.Shop?.Id ?? string.Empty,
            ShopName = product.Shop?.Name ?? string.Empty,
            Price = product.Price ?? 0,
            ProductId = product.Id,
            ProductName = product.Name ?? string.Empty,
            Variants = product.Variants.Select(v => new VariantForProductDto
            {
                VariantId = v.Id.ToString(),
                Price = v.Price ?? 0,
                Quantity = (int)(v.Quantity ?? 0),
                VariantValues = v.VariantValues.Select(vv => new VariantValueForProductDto
                {
                    // Id = vv.Id.ToString(),
                    // VariantId = vv.VariantId.ToString(),
                    PropertyValue = new PropertyValueForProductDto
                    {
                        // Id = vv.PropertyValue?.Id.ToString() ?? string.Empty,
                        Value = vv.PropertyValue?.Value ?? string.Empty,
                        ImgUrl = vv.PropertyValue?.ImgUrl ?? string.Empty,
                        Level = vv.PropertyValue?.Level ?? 0,
                        // PropertyProductId = vv.PropertyValue?.PropertyProductId?.ToString() ?? string.Empty,
                        // PropertyId = vv.PropertyValue?.PropertyProduct?.Id?.ToString() ?? string.Empty,
                        PropertyName = vv.PropertyValue?.PropertyProduct?.Name ?? string.Empty
                    },
                    Property = new PropertyForProductDto
                    {
                        // Id = vv.PropertyValue?.PropertyProduct?.Id.ToString() ?? string.Empty,
                        Name = vv.PropertyValue?.PropertyProduct?.Name ?? string.Empty
                    }
                }).ToList()
            }).ToList()
        };
    }
}