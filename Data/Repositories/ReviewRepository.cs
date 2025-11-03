using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using PeShop.Models.Enums;
namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public class ReviewRepository : IReviewRepository
{
    private readonly PeShopDbContext _context;
    public ReviewRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<bool> HasReviewAsync(string orderId, string productId, string userId)
    {
        return await _context.Reviews
            .AnyAsync(x => x.OrderId == orderId && x.ProductId == productId && x.UserId == userId);
    }
    public async Task<bool> CreateReviewAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
    public async Task<List<Review>> GetReviewByProductAsync(string productId)
    {
        return await _context.Reviews
            .Include(x => x.User)
            .Include(x => x.Product)
            .ThenInclude(x => x.Shop)
            .Where(x => x.ProductId == productId)
            .ToListAsync();
    }
}