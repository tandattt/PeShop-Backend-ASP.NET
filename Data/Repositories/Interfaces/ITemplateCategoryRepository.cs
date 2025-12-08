using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface ITemplateCategoryRepository
{
    Task<TemplateCategory> CreateAsync(TemplateCategory templateCategory);
    Task<TemplateCategory?> GetByIdAsync(int id);
    Task<List<TemplateCategory>> GetAllAsync();
    Task<(List<TemplateCategory> Data, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<TemplateCategory> UpdateAsync(TemplateCategory templateCategory);
    Task<bool> DeleteAsync(int id);
}

