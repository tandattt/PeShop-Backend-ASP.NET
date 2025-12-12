namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
public class RankRepository : IRankRepository
{
    private readonly PeShopDbContext _context;
    public RankRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<Rank?> GetRankByTotalSpentAsync(decimal totalSpent)
    {
        return await _context.Ranks.AsNoTracking().FirstOrDefaultAsync(x => x.MinPrice <= totalSpent && x.MaxPrice >= totalSpent);
    }
}