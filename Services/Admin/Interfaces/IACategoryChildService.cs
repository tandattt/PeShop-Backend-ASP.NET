using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface IACategoryChildService
{
    Task<CategoryChildResponse> CreateAsync(CreateCategoryChildRequest request);
    Task<CategoryChildResponse> GetByIdAsync(string id);
    Task<List<CategoryChildResponse>> GetAllAsync();
    Task<CategoryChildResponse> UpdateAsync(string id, UpdateCategoryChildRequest request);
    Task<StatusResponse> DeleteAsync(string id);
}

