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
        }).FirstOrDefaultAsync(x => x.Id == id) ?? new Shop();
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
        return await _context.Shops.FirstOrDefaultAsync(s => s.Id == shopId);
    }
    public async Task<Shop?> GetShopByUserIdAsync(string userId)
    {
        return await _context.Shops.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
