using PeShop.Models.Entities;
using PeShop.Dtos.Requests;
namespace PeShop.Data.Repositories.Admin.Interfaces;
public interface IAProductRepository
{
    Task<int> GetCountProductAsync(AGetProductRequest request);
    Task<List<Product>> GetListProductAsync(AGetProductRequest request);
}