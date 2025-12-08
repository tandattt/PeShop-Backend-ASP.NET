using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Services.Admin.Interfaces;

public interface IATemplateCategoryChildService
{
    Task<TemplateCategoryChildResponse> CreateAsync(CreateTemplateCategoryChildRequest request);
    Task<TemplateCategoryChildResponse> GetByIdAsync(int id);
    Task<List<TemplateCategoryChildResponse>> GetAllAsync();
    Task<PaginationResponse<TemplateCategoryChildResponse>> GetAllAsync(AGetTemplateCategoryChildRequest request);
    Task<TemplateCategoryChildResponse> UpdateAsync(int id, UpdateTemplateCategoryChildRequest request);
    Task<StatusResponse> DeleteAsync(int id);
}

