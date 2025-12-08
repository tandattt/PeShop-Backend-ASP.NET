using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace PeShop.Data.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly PeShopDbContext _context;
    public CategoryRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsDeleted == null || c.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<(List<Category> Data, int TotalCount)> GetCategoriesAsync(int page, int pageSize, string? search)
    {
        var query = _context.Categories
            .Where(c => c.IsDeleted == null || c.IsDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => 
                (c.Name != null && c.Name.ToLower().Contains(search)) ||
                (c.Type != null && c.Type.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> GetByIdAsync(string id)
    {
        return await _context.Categories
            .Where(c => c.Id == id && (c.IsDeleted == null || c.IsDeleted == false))
            .FirstOrDefaultAsync();
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }
        
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}