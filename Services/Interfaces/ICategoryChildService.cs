using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;
public interface ICategoryChildService
{
    Task<CategoryChildListResponse> GetCategoryChildrenAsync(string categoryId);
}
