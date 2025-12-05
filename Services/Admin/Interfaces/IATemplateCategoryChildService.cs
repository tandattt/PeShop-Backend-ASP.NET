using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface IATemplateCategoryChildService
{
    Task<TemplateCategoryChildResponse> CreateAsync(CreateTemplateCategoryChildRequest request);
    Task<TemplateCategoryChildResponse> GetByIdAsync(int id);
    Task<List<TemplateCategoryChildResponse>> GetAllAsync();
    Task<TemplateCategoryChildResponse> UpdateAsync(int id, UpdateTemplateCategoryChildRequest request);
    Task<StatusResponse> DeleteAsync(int id);
}

