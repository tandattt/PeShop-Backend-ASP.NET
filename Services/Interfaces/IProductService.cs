using PeShop.Dtos.Shared;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetFirstListProductAsync(string userId);
    Task<List<ProductDto>> GetNextListProductAsync(string userId);
    Task<ProductDetailResponse> GetProductDetailAsync(string productId);    
}
