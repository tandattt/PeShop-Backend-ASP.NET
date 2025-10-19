using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Services.Interfaces;
namespace PeShop.Services;
public class CategoryChildService : ICategoryChildService
{
    private readonly ICategoryChildRepository _categoryChildRepository;
    public CategoryChildService(ICategoryChildRepository categoryChildRepository)
    {
        _categoryChildRepository = categoryChildRepository;
    }
    public async Task<CategoryChildListResponse> GetCategoryChildrenAsync(string categoryId)
    {
        var categoryChildren = await _categoryChildRepository.GetCategoryChildrenAsync(categoryId);
        return new CategoryChildListResponse
        {
            CategoryChildren = categoryChildren.Select(c => new CategoryChildResponse
            {
                Id = c.Id,
                Name = c.Name ?? string.Empty,
            }).ToList()
        };
    }
}
