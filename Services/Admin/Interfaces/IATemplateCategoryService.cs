using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface IATemplateCategoryService
{
    Task<TemplateCategoryResponse> CreateAsync(CreateTemplateCategoryRequest request);
    Task<TemplateCategoryResponse> GetByIdAsync(int id);
    Task<List<TemplateCategoryResponse>> GetAllAsync();
    Task<TemplateCategoryResponse> UpdateAsync(int id, UpdateTemplateCategoryRequest request);
    Task<StatusResponse> DeleteAsync(int id);
}

