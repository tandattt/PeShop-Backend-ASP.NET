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
        return await _context.Categories.ToListAsync();
    }
}