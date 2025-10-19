using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Services.Interfaces;
namespace PeShop.Services;
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<CategoryListResponse> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        return new CategoryListResponse
        {
            Categories = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
            }).ToList()
        };
    }
}