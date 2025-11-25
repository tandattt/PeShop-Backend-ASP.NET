using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using PeShop.Models.Enums;

namespace PeShop.Data.Repositories;
public class FlashSaleRepository : IFlashSaleRepository
{
    private readonly PeShopDbContext _context;
    public FlashSaleRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<List<FlashSaleProduct>> GetFlashSalesAsync(int page, int pageSize, string flashSaleId)
    {
        return await _context.FlashSaleProducts
        .Include(f => f.Product)
        .Include(f => f.FlashSale)
        .Where(f => f.FlashSale != null && f.FlashSale.Id == flashSaleId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    }
    public async Task<List<FlashSale>> GetFlashSalesTodayAsync(DateOnly dateTime)
    {
        var startDate = dateTime.ToDateTime(TimeOnly.MinValue);
        var endDate = dateTime.ToDateTime(TimeOnly.MaxValue);
        return await _context.FlashSales
        .Where(f => f.StartTime <= startDate && f.EndTime >= endDate)
        .ToListAsync();
    }
    
    public async Task<Dictionary<string, bool>> HasFlashSalesForProductsAsync(List<string> productIds)
    {
        if (productIds == null || !productIds.Any())
        {
            return new Dictionary<string, bool>();
        }

        // Filter out null/empty productIds
        var validProductIds = productIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        if (!validProductIds.Any())
        {
            return new Dictionary<string, bool>();
        }

        var currentTime = DateTime.Now;

        // Debug: Query tất cả flash sale products không check điều kiện
        var allFlashSaleProducts = await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.ProductId != null && validProductIds.Contains(fsp.ProductId))
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Total flash sale products found: {allFlashSaleProducts.Count}");
        foreach (var fsp in allFlashSaleProducts)
        {
            Console.WriteLine($"[DEBUG] ProductId: {fsp.ProductId}, FlashSaleId: {fsp.FlashSaleId}, Status: {fsp.FlashSale?.Status}, StartTime: {fsp.FlashSale?.StartTime}, EndTime: {fsp.FlashSale?.EndTime}, CurrentTime: {currentTime}");
        }

        var productsWithFlashSales = await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.ProductId != null 
                        && validProductIds.Contains(fsp.ProductId)
                        && fsp.Product != null && fsp.Product.Status == ProductStatus.Active
                        && fsp.FlashSale != null 
                        && fsp.FlashSale.Status == FlashSaleStatus.Active
                        && fsp.FlashSale.StartTime <= currentTime
                        && fsp.FlashSale.EndTime >= currentTime)
            .Select(fsp => fsp.ProductId!)
            .Distinct()
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Products with active flash sales: {productsWithFlashSales.Count}");

        // Tạo dictionary với tất cả productIds, mặc định false
        var result = validProductIds.ToDictionary(id => id, id => false);
        
        // Set true cho các products có flash sale đang active
        foreach (var productId in productsWithFlashSales)
        {
            if (!string.IsNullOrEmpty(productId) && result.ContainsKey(productId))
            {
                result[productId] = true;
            }
        }

        return result;
    }
    
    public async Task<Dictionary<string, uint>> GetFlashSaleDiscountsForProductsAsync(List<string> productIds)
    {
        if (productIds == null || !productIds.Any())
        {
            return new Dictionary<string, uint>();
        }

        // Filter out null/empty productIds
        var validProductIds = productIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        if (!validProductIds.Any())
        {
            return new Dictionary<string, uint>();
        }

        var currentTime = DateTime.Now;

        var flashSaleDiscounts = await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.ProductId != null 
                        && validProductIds.Contains(fsp.ProductId)
                        && fsp.Product != null && fsp.Product.Status == ProductStatus.Active
                        && fsp.FlashSale != null 
                        && fsp.FlashSale.Status == FlashSaleStatus.Active
                        && fsp.FlashSale.StartTime <= currentTime
                        && fsp.FlashSale.EndTime >= currentTime
                        && fsp.PercentDecrease != null)
            .Select(fsp => new { ProductId = fsp.ProductId!, PercentDecrease = fsp.PercentDecrease!.Value })
            .ToListAsync();

        // Tạo dictionary với productId và percent_decrease
        var result = new Dictionary<string, uint>();
        foreach (var item in flashSaleDiscounts)
        {
            if (!string.IsNullOrEmpty(item.ProductId) && !result.ContainsKey(item.ProductId))
            {
                result[item.ProductId] = item.PercentDecrease;
            }
        }

        return result;
    }
}