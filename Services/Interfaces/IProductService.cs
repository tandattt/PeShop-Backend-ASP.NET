using PeShop.Dtos.Shared;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailResponse> GetProductDetailAsync(string? productId, string? slug);
    Task<PaginationResponse<ProductDto>> GetProductsByShopAsync(GetProductByShopRequest request);
    // Pagination method
    Task<PaginationResponse<ProductDto>> GetProductsAsync(GetProductRequest request);

}
