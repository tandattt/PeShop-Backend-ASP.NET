using PeShop.Dtos.Shared;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailResponse> GetProductDetailAsync(string? productId, string? slug);
    
    // Pagination method
    Task<PaginationResponse<ProductDto>> GetProductsWithPaginationAsync(PaginationRequest request);
}
