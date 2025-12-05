using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface IACategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> GetByIdAsync(string id);
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> UpdateAsync(string id, UpdateCategoryRequest request);
    Task<StatusResponse> DeleteAsync(string id);
}

