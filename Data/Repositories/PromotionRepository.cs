using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using PeShop.Models.Enums;
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
        .Where(p => p.ProductId == productId
            && p.Promotion!.ShopId == shopId
            && p.Product!.Status == ProductStatus.Active
            && p.Promotion!.Status == PromotionStatus.Active)
        .ToListAsync();
        return products.Select(p => p.Product).Where(p => p != null).ToList();
    }
    public async Task<List<Promotion?>> GetPromotionByProductAsync(string productId)
    {
        var promotions = await _context.Promotions
        .Include(p => p.PromotionRules)
            .ThenInclude(r => r.Product)
        .Include(p => p.PromotionGifts)
            .ThenInclude(g => g.Product)
        .Where(p => p.Status == PromotionStatus.Active)
        .Where(p => p.PromotionRules.Any(r => r.ProductId == productId && r.Product != null && r.Product.Status == ProductStatus.Active))
        .ToListAsync();
        Console.WriteLine(string.Join(", ", promotions.Select(p => p.Id)));
        return promotions.Cast<Promotion?>().ToList();
    }
    public async Task<bool> HasPromotionAsync(string productId)
    {
        return await _context.PromotionRules
            .Include(r => r.Product)
            .Include(r => r.Promotion)
            .AnyAsync(r => r.ProductId == productId
                        && r.Product != null && r.Product.Status == ProductStatus.Active
                        && r.Promotion != null && r.Promotion.Status == PromotionStatus.Active);
    }
    public async Task<List<Promotion?>> GetPromotionsByShopAsync(string shopId)
    {
        var promotions = await _context.Promotions
            .Include(p => p.PromotionRules)
                .ThenInclude(r => r.Product)
            .Include(p => p.PromotionGifts)
                .ThenInclude(g => g.Product)
            .Where(p => p.ShopId == shopId && p.Status == PromotionStatus.Active)
            .Select(p => new {
                Promotion = p,
                // filter out inactive products in rules and gifts
                Rules = p.PromotionRules.Where(r => r.Product != null && r.Product.Status == ProductStatus.Active).ToList(),
                Gifts = p.PromotionGifts.Where(g => g.IsDeleted != true && (g.Product == null || g.Product.Status == ProductStatus.Active)).ToList()
            })
            .AsNoTracking()
            .ToListAsync();

        // Rehydrate promotions with filtered collections to avoid inactive products leaking
        var result = promotions.Select(x => {
            x.Promotion.PromotionRules = x.Rules;
            x.Promotion.PromotionGifts = x.Gifts;
            return x.Promotion;
        }).ToList();
        return result.Cast<Promotion?>().ToList();
    }
    public async Task<Promotion?> GetPromotionByIdAsync(string promotionId)
    {
        return await _context.Promotions.FindAsync(promotionId);
    }
    public async Task<bool> UpdatePromotionAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
    public async Task<bool> CreatePromotionUsageAsync(PromotionUsage promotionUsage)
    {
        await _context.PromotionUsages.AddAsync(promotionUsage);
        
        // // Tăng used_count trong Promotion nếu có PromotionId
        // if (!string.IsNullOrEmpty(promotionUsage.PromotionId))
        // {
        //     var promotion = await _context.Promotions.FindAsync(promotionUsage.PromotionId);
        //     if (promotion != null)
        //     {
        //         promotion.UsedCount = (promotion.UsedCount ?? 0) + 1;
        //         promotion.UpdatedAt = DateTime.UtcNow;
        //         _context.Promotions.Update(promotion);
        //     }
        // }
        
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }

    public async Task<bool> CreatePromotionUsageWithoutIncrementAsync(PromotionUsage promotionUsage)
    {
        // Tạo PromotionUsage mà không tăng used_count (đã được tăng trước đó)
        await _context.PromotionUsages.AddAsync(promotionUsage);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
}