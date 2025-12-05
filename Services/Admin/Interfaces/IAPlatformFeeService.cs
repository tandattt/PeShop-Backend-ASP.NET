using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface IAPlatformFeeService
{
    Task<PlatformFeeResponse> CreateAsync(CreatePlatformFeeRequest request);
    Task<PlatformFeeResponse> GetByIdAsync(uint id);
    Task<List<PlatformFeeResponse>> GetAllAsync();
    Task<List<PlatformFeeResponse>> GetByCategoryIdAsync(string categoryId);
    Task<PlatformFeeResponse> UpdateAsync(uint id, UpdatePlatformFeeRequest request);
}

