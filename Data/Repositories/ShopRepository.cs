using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly PeShopDbContext _context;
    public ShopRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<Shop> GetAddressShopById(string id)
    {
        return await _context.Shops.Select(x => new Shop
        {
            Id = x.Id,
            FullNewAddress = x.FullNewAddress,
            NewProviceId = x.NewProviceId,
            NewWardId = x.NewWardId,
            OldDistrictId = x.OldDistrictId,
            OldProviceId = x.OldProviceId,
            OldWardId = x.OldWardId,
        }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? new Shop();
    }

    public async Task<List<Shop>> SearchShopsAsync(string keyword, int skip = 0, int take = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<Shop>();
        }

        var searchTerm = keyword.ToLower().Trim();

        return await _context.Shops
            .Where(s => s.Name != null && s.Name.ToLower().Contains(searchTerm))
            .OrderByDescending(s => s.FollowersCount)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetSearchShopsCountAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return 0;
        }

        var searchTerm = keyword.ToLower().Trim();

        return await _context.Shops
            .Where(s => s.Name != null && s.Name.ToLower().Contains(searchTerm))
            .CountAsync();
    }

    public async Task<Shop?> GetShopByIdAsync(string shopId)
    {
        return await _context.Shops
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shopId);
    }
    public async Task<Shop?> GetShopByUserIdAsync(string userId)
    {
        return await _context.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<string?> GetShopIdByProductIdAsync(string productId)
    {
        return await _context.Products
            .Where(x => x.Id == productId)
            .Select(x => x.ShopId)
            .AsNoTracking()
            .FirstAsync();
    }
}
