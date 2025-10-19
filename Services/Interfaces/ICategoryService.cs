using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;
public interface ICategoryService
{
    Task<CategoryListResponse> GetCategoriesAsync();
}