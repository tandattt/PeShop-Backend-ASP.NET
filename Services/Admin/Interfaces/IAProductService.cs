using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
namespace PeShop.Services.Admin.Interfaces;

public interface IAProductService
{
    Task<PaginationResponse<ProductDto>> GetProductsAsync(AGetProductRequest request);
    Task<PaginationResponse<ProductDto>> GetProductsApprovalAsync(AGetProductRequest request);
    Task<StatusResponse> ApproveProductAsync(ApproveProductRequest request);
}