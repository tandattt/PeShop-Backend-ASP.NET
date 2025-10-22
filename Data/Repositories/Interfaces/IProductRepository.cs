using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> GetCountProductAsync();
    Task<List<Product>> GetListProductAsync(int skip, int take);
    Task<Product> GetProductByIdAsync(string productId);
    Task<Product> GetProductBySlugAsync(string slug);
    Task<ProductShippingDto?> GetProductForShippingByIdAsync(string productId);
    Task<List<Product>> SearchProductsAsync(string keyword, int skip = 0, int take = 20);
    Task<int> GetSearchProductsCountAsync(string keyword);
    Task<List<Product>> GetListProductByAsync(GetProductRequest request);
    Task<int> GetCountProductByAsync(GetProductRequest request);
    Task<List<Product>> GetListProductByShopAsync(GetProductByShopRequest request);
    Task<int> GetCountProductByShopAsync(GetProductByShopRequest request);
    
}
