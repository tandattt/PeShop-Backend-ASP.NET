using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PeShopDbContext _context;
    public ProductRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<int> GetCountProductAsync(){
        return await _context.Products.CountAsync();
    }
    public async Task<List<Product>> GetListProductAsync(int skip, int take)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    public async Task<Product> GetProductByIdAsync(string productId)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.CategoryChild)
            .Include(p => p.Variants)
                .ThenInclude(v => v.VariantValues)
                    .ThenInclude(vv => vv.PropertyValue)
                        .ThenInclude(pv => pv.PropertyProduct)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<Product> GetProductBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.CategoryChild)
            .Include(p => p.Variants)
            .ThenInclude(v => v.VariantValues)
                    .ThenInclude(vv => vv.PropertyValue)
                        .ThenInclude(pv => pv.PropertyProduct)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<ProductShippingDto?> GetProductForShippingByIdAsync(string productId)
    {
        try
        {
            // First check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            Console.WriteLine($"Product '{productId}' exists in database: {productExists}");
            
            if (!productExists)
            {
                return null;
            }
            
            var result = await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => new ProductShippingDto
                {
                    Price = p.Price,
                    Height = p.Height,
                    Length = p.Length,
                    Width = p.Width,
                    Weight = p.Weight
                })
                .FirstOrDefaultAsync();
                
            // Debug logging
            if (result == null)
            {
                Console.WriteLine($"Product with ID '{productId}' projection returned null");
            }
            else
            {
                Console.WriteLine($"Found product: Price={result.Price}, Height={result.Height}, Length={result.Length}, Width={result.Width}, Weight={result.Weight}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProductForShippingByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Product>> SearchProductsAsync(string keyword, int skip = 0, int take = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<Product>();
        }

        var searchTerm = keyword.ToLower().Trim();
        
        return await _context.Products
            .Include(p => p.Shop)
            .Where(p => p.Name != null && p.Name.ToLower().Contains(searchTerm))
            .OrderByDescending(p => p.BoughtCount)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetSearchProductsCountAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return 0;
        }

        var searchTerm = keyword.ToLower().Trim();
        
        return await _context.Products
            .Where(p => p.Name != null && p.Name.ToLower().Contains(searchTerm))
            .CountAsync();
    }
    public async Task<List<Product>> GetListProductByAsync(GetProductRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).AsQueryable();

        // Filter by CategoryId if not null
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }

        // Filter by CategoryChildId if not null
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId);
        }

        // Filter by price range
        query = query.Where(p => p.Price >= request.MinPrice);
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        // Filter by review point if not null
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint >= request.ReviewPoint.Value);
        }

        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
    }
    public async Task<int> GetCountProductByAsync(GetProductRequest request)
    {
        var query = _context.Products.AsQueryable();

        // Filter by CategoryId if not null
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }

        // Filter by CategoryChildId if not null
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId);
        }

        // Filter by price range
        query = query.Where(p => p.Price >= request.MinPrice);
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        // Filter by review point if not null
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint >= request.ReviewPoint.Value);
        }

        return await query.CountAsync();
    }
    public async Task<List<Product>> GetListProductByShopAsync(GetProductByShopRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).Where(p => p.ShopId == request.ShopId).AsQueryable();
        return await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();
    }
    public async Task<int> GetCountProductByShopAsync(GetProductByShopRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).Where(p => p.ShopId == request.ShopId).AsQueryable();
        return await query.CountAsync();
    }
}