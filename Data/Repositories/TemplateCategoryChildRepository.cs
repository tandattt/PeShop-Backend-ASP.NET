using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;

namespace PeShop.Data.Repositories;

public class TemplateCategoryChildRepository : ITemplateCategoryChildRepository
{
    private readonly PeShopDbContext _context;

    public TemplateCategoryChildRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateCategoryChild> CreateAsync(TemplateCategoryChild templateCategoryChild)
    {
        _context.TemplateCategoryChildren.Add(templateCategoryChild);
        await _context.SaveChangesAsync();
        return templateCategoryChild;
    }

    public async Task<TemplateCategoryChild?> GetByIdAsync(int id)
    {
        return await _context.TemplateCategoryChildren
            .Where(tcc => tcc.Id == id && (tcc.IsDeleted == null || tcc.IsDeleted == false))
            .Include(tcc => tcc.CategoryChild)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<TemplateCategoryChild>> GetAllAsync()
    {
        return await _context.TemplateCategoryChildren
            .Where(tcc => tcc.IsDeleted == null || tcc.IsDeleted == false)
            .Include(tcc => tcc.CategoryChild)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<TemplateCategoryChild> Data, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var query = _context.TemplateCategoryChildren
            .Where(tcc => tcc.IsDeleted == null || tcc.IsDeleted == false);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(tcc => tcc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<TemplateCategoryChild> UpdateAsync(TemplateCategoryChild templateCategoryChild)
    {
        _context.TemplateCategoryChildren.Update(templateCategoryChild);
        await _context.SaveChangesAsync();
        return templateCategoryChild;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var templateCategoryChild = await _context.TemplateCategoryChildren.FindAsync(id);
        if (templateCategoryChild == null)
        {
            return false;
        }
        
        templateCategoryChild.IsDeleted = true;
        templateCategoryChild.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}

