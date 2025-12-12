using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using Models.Enums;
namespace PeShop.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> GetCountProductAsync();
    Task<List<Product>> GetListProductAsync(int page, int pageSize);
    Task<Product?> GetProductByIdAsync(string productId);
    Task<Product?> GetProductBySlugAsync(string slug);
    Task<ProductShippingDto?> GetProductForShippingByIdAsync(string productId);
    Task<List<Product>> SearchProductsAsync(string keyword, int skip = 0, int take = 20);
    Task<int> GetSearchProductsCountAsync(string keyword);
    Task<List<Product>> GetListProductByAsync(GetProductRequest request);
    Task<int> GetCountProductByAsync(GetProductRequest request);
    Task<List<Product>> GetListProductByShopAsync(GetProductByShopRequest request);
    Task<int> GetCountProductByShopAsync(GetProductByShopRequest request);
    Task<List<Product>> GetListProductByCategoryChildIdAsync(string? categoryChildId);
    Task<List<Product>> GetListProductByVectorAsync(List<string> productIds);
    Task<bool> UpdateProductAsync(Product product);
    Task<List<Product>> GetProductsByStatusAsync(ProductStatus status);
    Task<bool> UpdateProductStatusAsync(string productId, ProductStatus status, string? reason);
}
