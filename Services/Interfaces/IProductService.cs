using PeShop.Dtos.Shared;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Dtos.API;
namespace PeShop.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailResponse> GetProductDetailAsync(string? productId, string? slug);
    Task<PaginationResponse<ProductDto>> GetProductsByShopAsync(GetProductByShopRequest request);
    // Pagination method
    Task<RecomemtProductDto> GetRecomemtProductsAsync(string product_id);
    Task<PaginationResponse<ProductDto>> GetProductsAsync(GetProductRequest request);
   

}
