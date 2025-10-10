using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
namespace PeShop.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> GetCountProductAsync();
    Task<List<Product>> GetListProductAsync(int skip, int take);
    Task<Product> GetProductByIdAsync(string productId);
    Task<ProductShippingDto?> GetProductForShippingByIdAsync(string productId);
}
