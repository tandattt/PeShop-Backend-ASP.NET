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
        }).FirstOrDefaultAsync(x => x.Id == id) ?? new Shop();
    }
}
