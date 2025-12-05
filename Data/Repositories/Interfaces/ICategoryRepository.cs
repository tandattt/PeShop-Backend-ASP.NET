namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync();
    Task<Category> CreateAsync(Category category);
    Task<Category?> GetByIdAsync(string id);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
}