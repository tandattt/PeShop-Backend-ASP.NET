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

    public async Task<FlashSaleProduct?> GetActiveFlashSaleProductAsync(string productId)
    {
        var currentTime = DateTime.Now;
        
        return await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.ProductId == productId
                        && fsp.Product != null && fsp.Product.Status == ProductStatus.Active
                        && fsp.FlashSale != null 
                        && fsp.FlashSale.Status == FlashSaleStatus.Active
                        && fsp.FlashSale.StartTime <= currentTime
                        && fsp.FlashSale.EndTime >= currentTime)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<string, FlashSaleProduct>> GetFlashSaleProductsByIdsAsync(List<string> flashSaleProductIds)
    {
        if (flashSaleProductIds == null || !flashSaleProductIds.Any())
        {
            return new Dictionary<string, FlashSaleProduct>();
        }

        var flashSaleProducts = await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.Id != null && flashSaleProductIds.Contains(fsp.Id))
            .ToListAsync();

        return flashSaleProducts.ToDictionary(fsp => fsp.Id, fsp => fsp);
    }

    public async Task<Dictionary<string, FlashSaleProduct>> GetActiveFlashSaleProductsAsync(List<string> productIds)
    {
        if (productIds == null || !productIds.Any())
        {
            return new Dictionary<string, FlashSaleProduct>();
        }

        var currentTime = DateTime.Now;

        var flashSaleProducts = await _context.FlashSaleProducts
            .Include(fsp => fsp.Product)
            .Include(fsp => fsp.FlashSale)
            .Where(fsp => fsp.ProductId != null 
                        && productIds.Contains(fsp.ProductId)
                        && fsp.Product != null && fsp.Product.Status == ProductStatus.Active
                        && fsp.FlashSale != null 
                        && fsp.FlashSale.Status == FlashSaleStatus.Active
                        && fsp.FlashSale.StartTime <= currentTime
                        && fsp.FlashSale.EndTime >= currentTime)
            .ToListAsync();

        return flashSaleProducts
            .Where(fsp => fsp.ProductId != null)
            .ToDictionary(fsp => fsp.ProductId!, fsp => fsp);
    }

    public async Task<bool> DecreaseFlashSaleQuantityAsync(string flashSaleProductId, uint quantity)
    {
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE flash_sale_product 
              SET used_quantity = used_quantity + {0}
              WHERE id = {1} 
              AND (quantity - COALESCE(used_quantity, 0)) >= {0}",
            quantity, flashSaleProductId);
        
        return rowsAffected > 0;
    }

    public async Task<int> GetUserFlashSalePurchaseCountAsync(string userId, string flashSaleProductId)
    {
        var count = await _context.OrderDetails
            .Where(od => od.Order != null 
                      && od.Order.UserId == userId
                      && od.FlashSaleProductId == flashSaleProductId
                      && od.Order.StatusPayment != PaymentStatus.Failed
                      && od.Order.StatusPayment != PaymentStatus.Refunded)
            .SumAsync(od => (int?)od.Quantity) ?? 0;
        
        return count;
    }
}