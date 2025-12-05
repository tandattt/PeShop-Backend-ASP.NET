using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface ITemplateCategoryChildRepository
{
    Task<TemplateCategoryChild> CreateAsync(TemplateCategoryChild templateCategoryChild);
    Task<TemplateCategoryChild?> GetByIdAsync(int id);
    Task<List<TemplateCategoryChild>> GetAllAsync();
    Task<TemplateCategoryChild> UpdateAsync(TemplateCategoryChild templateCategoryChild);
    Task<bool> DeleteAsync(int id);
}

