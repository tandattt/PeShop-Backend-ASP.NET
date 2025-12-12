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
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<HashSet<(string OrderId, string ProductId)>> GetExistingReviewsBatchAsync(List<(string OrderId, string ProductId)> items, string userId)
    {
        if (items == null || !items.Any())
        {
            return new HashSet<(string OrderId, string ProductId)>();
        }

        var orderIds = items.Select(x => x.OrderId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        var productIds = items.Select(x => x.ProductId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

        if (!orderIds.Any() || !productIds.Any())
        {
            return new HashSet<(string OrderId, string ProductId)>();
        }

        var existingReviews = await _context.Reviews
            .Where(x => x.UserId == userId 
                && x.OrderId != null
                && x.ProductId != null
                && orderIds.Contains(x.OrderId) 
                && productIds.Contains(x.ProductId))
            .Select(x => new { OrderId = x.OrderId!, ProductId = x.ProductId! })
            .AsNoTracking()
            .ToListAsync();

        return existingReviews.Select(x => (x.OrderId, x.ProductId)).ToHashSet();
    }
}