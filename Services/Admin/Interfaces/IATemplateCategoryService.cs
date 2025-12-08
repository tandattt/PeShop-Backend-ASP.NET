using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Services.Admin.Interfaces;

public interface IATemplateCategoryService
{
    Task<TemplateCategoryResponse> CreateAsync(CreateTemplateCategoryRequest request);
    Task<TemplateCategoryResponse> GetByIdAsync(int id);
    Task<List<TemplateCategoryResponse>> GetAllAsync();
    Task<PaginationResponse<TemplateCategoryResponse>> GetAllAsync(AGetTemplateCategoryRequest request);
    Task<TemplateCategoryResponse> UpdateAsync(int id, UpdateTemplateCategoryRequest request);
    Task<StatusResponse> DeleteAsync(int id);
}

