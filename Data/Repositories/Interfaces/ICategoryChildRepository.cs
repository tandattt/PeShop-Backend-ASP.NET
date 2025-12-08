namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface ICategoryChildRepository
{
    Task<List<CategoryChild>> GetCategoryChildrenAsync(string categoryId);
    Task<CategoryChild> CreateAsync(CategoryChild categoryChild);
    Task<CategoryChild?> GetByIdAsync(string id);
    Task<List<CategoryChild>> GetAllAsync();
    Task<(List<CategoryChild> Data, int TotalCount)> GetAllAsync(int page, int pageSize, string? search);
    Task<CategoryChild> UpdateAsync(CategoryChild categoryChild);
    Task<bool> DeleteAsync(string id);
}
