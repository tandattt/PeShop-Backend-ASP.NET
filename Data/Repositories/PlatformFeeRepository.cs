namespace PeShop.Data.Repositories;
using PeShop.Models.Entities;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
public class PlatformFeeRepository : IPlatformFeeRepository
{
    private readonly PeShopDbContext _context;
    public PlatformFeeRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<decimal> GetPlatformFeeByCategoryIdAsync(string categoryId)
    {
        return await _context.PlatformFees.Where(p => p.CategoryId == categoryId && p.IsActive == true).Select(p => p.Fee).FirstOrDefaultAsync();
    }
}   