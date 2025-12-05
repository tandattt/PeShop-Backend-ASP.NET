using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;

namespace PeShop.Data.Repositories;

public class TemplateCategoryRepository : ITemplateCategoryRepository
{
    private readonly PeShopDbContext _context;

    public TemplateCategoryRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateCategory> CreateAsync(TemplateCategory templateCategory)
    {
        _context.TemplateCategories.Add(templateCategory);
        await _context.SaveChangesAsync();
        return templateCategory;
    }

    public async Task<TemplateCategory?> GetByIdAsync(int id)
    {
        return await _context.TemplateCategories
            .Where(tc => tc.Id == id && (tc.IsDeleted == null || tc.IsDeleted == false))
            .FirstOrDefaultAsync();
    }

    public async Task<List<TemplateCategory>> GetAllAsync()
    {
        return await _context.TemplateCategories
            .Where(tc => tc.IsDeleted == null || tc.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<TemplateCategory> UpdateAsync(TemplateCategory templateCategory)
    {
        _context.TemplateCategories.Update(templateCategory);
        await _context.SaveChangesAsync();
        return templateCategory;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var templateCategory = await _context.TemplateCategories.FindAsync(id);
        if (templateCategory == null)
        {
            return false;
        }
        
        templateCategory.IsDeleted = true;
        templateCategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}

