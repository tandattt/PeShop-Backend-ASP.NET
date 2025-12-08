using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface ITemplateCategoryChildRepository
{
    Task<TemplateCategoryChild> CreateAsync(TemplateCategoryChild templateCategoryChild);
    Task<TemplateCategoryChild?> GetByIdAsync(int id);
    Task<List<TemplateCategoryChild>> GetAllAsync();
    Task<(List<TemplateCategoryChild> Data, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<TemplateCategoryChild> UpdateAsync(TemplateCategoryChild templateCategoryChild);
    Task<bool> DeleteAsync(int id);
}

