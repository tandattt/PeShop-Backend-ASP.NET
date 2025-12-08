using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Services.Admin.Interfaces;

public interface IAPlatformFeeService
{
    Task<PlatformFeeResponse> CreateAsync(CreatePlatformFeeRequest request);
    Task<PlatformFeeResponse> GetByIdAsync(uint id);
    Task<List<PlatformFeeResponse>> GetAllAsync();
    Task<PaginationResponse<PlatformFeeResponse>> GetAllAsync(AGetPlatformFeeRequest request);
    Task<List<PlatformFeeResponse>> GetByCategoryIdAsync(string categoryId);
    Task<PaginationResponse<PlatformFeeResponse>> GetByCategoryIdAsync(string categoryId, AGetPlatformFeeRequest request);
    Task<PlatformFeeResponse> UpdateAsync(uint id, UpdatePlatformFeeRequest request);
}

