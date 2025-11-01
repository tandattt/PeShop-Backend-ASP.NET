using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
namespace PeShop.Data.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly PeShopDbContext _context;
    public PromotionRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<List<Product?>> GetProductInPromotionAsync(string productId, string shopId)
    {
        var products = await _context.PromotionRules
        .Include(p => p.Product)
        .Include(p => p.Promotion)
        .Where(p => p.ProductId == productId && p.Promotion!.ShopId == shopId).ToListAsync();
        return products.Select(p => p.Product).Where(p => p != null).ToList();
    }
    public async Task<List<Promotion?>> GetPromotionByProductAsync(string productId)
    {
        var promotions = await _context.Promotions
        .Include(p => p.PromotionRules)
            .ThenInclude(p => p.Product)
        .Include(p => p.PromotionGifts)
            .ThenInclude(p => p.Product)
        .Where(p => p.PromotionRules.Any(r => r.ProductId == productId)).ToListAsync();
        Console.WriteLine(string.Join(", ", promotions.Select(p => p.Id)));
        return promotions;
    }
    public async Task<bool> HasPromotionAsync(string productId){
        return await _context.PromotionRules.AnyAsync(p => p.ProductId == productId);
    }
}