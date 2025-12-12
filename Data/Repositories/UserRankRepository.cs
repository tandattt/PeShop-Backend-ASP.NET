namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
public class UserRankRepository : IUserRankRepository
{
    private readonly PeShopDbContext _context;
    public UserRankRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<UserRank?> GetUserRankByIdAsync(string userId)
    {
        return await _context.UserRanks.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }
    
    public async Task<UserRank?> UpdateUserRankAsync(UserRank userRank)
    {
        _context.UserRanks.Update(userRank);
        await _context.SaveChangesAsync();
        return userRank;
    }
    public async Task<bool> CreateUserRankAsync(UserRank userRank)
    {
        await _context.UserRanks.AddAsync(userRank);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
}