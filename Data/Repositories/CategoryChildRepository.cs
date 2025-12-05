using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace PeShop.Data.Repositories;
public class CategoryChildRepository : ICategoryChildRepository
{
    private readonly PeShopDbContext _context;
    public CategoryChildRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<List<CategoryChild>> GetCategoryChildrenAsync(string categoryId)
    {
        return await _context.CategoryChildren
            .Where(cc => cc.CategoryId == categoryId && (cc.IsDeleted == null || cc.IsDeleted == false))
            .ToListAsync();
    }

    public async Task<CategoryChild> CreateAsync(CategoryChild categoryChild)
    {
        _context.CategoryChildren.Add(categoryChild);
        await _context.SaveChangesAsync();
        return categoryChild;
    }

    public async Task<CategoryChild?> GetByIdAsync(string id)
    {
        return await _context.CategoryChildren
            .Where(cc => cc.Id == id && (cc.IsDeleted == null || cc.IsDeleted == false))
            .FirstOrDefaultAsync();
    }

    public async Task<List<CategoryChild>> GetAllAsync()
    {
        return await _context.CategoryChildren
            .Where(cc => cc.IsDeleted == null || cc.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<CategoryChild> UpdateAsync(CategoryChild categoryChild)
    {
        _context.CategoryChildren.Update(categoryChild);
        await _context.SaveChangesAsync();
        return categoryChild;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var categoryChild = await _context.CategoryChildren.FindAsync(id);
        if (categoryChild == null)
        {
            return false;
        }
        
        categoryChild.IsDeleted = true;
        categoryChild.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
