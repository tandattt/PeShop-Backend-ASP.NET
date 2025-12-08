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

    public async Task<PlatformFee> CreateAsync(PlatformFee platformFee)
    {
        _context.PlatformFees.Add(platformFee);
        await _context.SaveChangesAsync();
        return platformFee;
    }

    public async Task<PlatformFee?> GetByIdAsync(uint id)
    {
        return await _context.PlatformFees.FindAsync(id);
    }

    public async Task<List<PlatformFee>> GetAllAsync()
    {
        return await _context.PlatformFees
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<PlatformFee> Data, int TotalCount)> GetAllAsync(int page, int pageSize, string? categoryId)
    {
        var query = _context.PlatformFees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<List<PlatformFee>> GetByCategoryIdAsync(string categoryId)
    {
        return await _context.PlatformFees
            .Where(p => p.CategoryId == categoryId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<PlatformFee> Data, int TotalCount)> GetByCategoryIdAsync(string categoryId, int page, int pageSize)
    {
        var query = _context.PlatformFees
            .Where(p => p.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<PlatformFee> UpdateAsync(PlatformFee platformFee)
    {
        _context.PlatformFees.Update(platformFee);
        await _context.SaveChangesAsync();
        return platformFee;
    }

    public async Task DeactivateOthersByCategoryIdAsync(string categoryId, uint excludeId)
    {
        var others = await _context.PlatformFees
            .Where(p => p.CategoryId == categoryId && p.Id != excludeId && p.IsActive == true)
            .ToListAsync();
        
        foreach (var item in others)
        {
            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
}   