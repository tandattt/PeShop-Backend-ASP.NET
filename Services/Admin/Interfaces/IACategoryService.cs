using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Services.Admin.Interfaces;

public interface IACategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> GetByIdAsync(string id);
    Task<List<CategoryResponse>> GetAllAsync();
    Task<PaginationResponse<CategoryResponse>> GetAllAsync(AGetCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(string id, UpdateCategoryRequest request);
    Task<StatusResponse> DeleteAsync(string id);
}

