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
            .Where(cc => cc.CategoryId == categoryId)
            .ToListAsync();
    }
}
