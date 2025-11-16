using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using Models.Enums;
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
            .OrderByDescending(p => p.score)
            .Include(p => p.Shop)
            .Skip(skip)
            .Take(take)
            .Where(p => p.Status == ProductStatus.Active)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<Product?> GetProductByIdAsync(string productId)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.CategoryChild)
            .Include(p => p.Variants)
                .ThenInclude(v => v.VariantValues)
                    .ThenInclude(vv => vv.PropertyValue)
                        .ThenInclude(pv => pv.PropertyProduct)
            .FirstOrDefaultAsync(p => p.Id == productId && p.Status == ProductStatus.Active);
    }

    public async Task<Product?> GetProductBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.CategoryChild)
            .Include(p => p.Variants)
            .ThenInclude(v => v.VariantValues)
                    .ThenInclude(vv => vv.PropertyValue)
                        .ThenInclude(pv => pv.PropertyProduct)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ProductStatus.Active);
    }

    public async Task<ProductShippingDto?> GetProductForShippingByIdAsync(string productId)
    {
        try
        {
            // First check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId && p.Status == ProductStatus.Active);
            Console.WriteLine($"Product '{productId}' exists in database: {productExists}");
            
            if (!productExists)
            {
                return null;
            }
            
            var result = await _context.Products
                .Where(p => p.Id == productId && p.Status == ProductStatus.Active)
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
            .Where(p => p.Status == ProductStatus.Active)
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
            .Where(p => p.Status == ProductStatus.Active)
            .CountAsync();
    }
    public async Task<List<Product>> GetListProductByAsync(GetProductRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).Where(p => p.Status == ProductStatus.Active).AsQueryable();

        // Filter by CategoryId if not null
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId && p.Status == ProductStatus.Active);
        }

        // Filter by CategoryChildId if not null
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId && p.Status == ProductStatus.Active);
        }

        // Filter by price range
        query = query.Where(p => p.Price >= request.MinPrice && p.Status == ProductStatus.Active);
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value && p.Status == ProductStatus.Active);
        }

        // Filter by review point if not null
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint >= request.ReviewPoint.Value && p.Status == ProductStatus.Active);
        }

        return await query
            .OrderByDescending(p => p.score)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<int> GetCountProductByAsync(GetProductRequest request)
    {
        var query = _context.Products.Where(p => p.Status == ProductStatus.Active).AsQueryable();

        // Filter by CategoryId if not null
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId && p.Status == ProductStatus.Active);
        }

        // Filter by CategoryChildId if not null
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId && p.Status == ProductStatus.Active);
        }

        // Filter by price range
        query = query.Where(p => p.Price >= request.MinPrice && p.Status == ProductStatus.Active    );
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value && p.Status == ProductStatus.Active);
        }

        // Filter by review point if not null
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint >= request.ReviewPoint.Value && p.Status == ProductStatus.Active);
        }

        return await query.CountAsync();
    }
    public async Task<List<Product>> GetListProductByShopAsync(GetProductByShopRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).Where(p => p.ShopId == request.ShopId && p.Status == ProductStatus.Active).AsQueryable();
        return await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).AsNoTracking().ToListAsync();
    }
    public async Task<int> GetCountProductByShopAsync(GetProductByShopRequest request)
    {
        var query = _context.Products.Include(p => p.Shop).Where(p => p.ShopId == request.ShopId && p.Status == ProductStatus.Active).AsQueryable();
        return await query.CountAsync();
    }


    public async Task<List<Product>> GetListProductByCategoryChildIdAsync(string? categoryChildId)
    {
        if (string.IsNullOrEmpty(categoryChildId))
        {
            return new List<Product>();
        }
        return await _context.Products.Include(p => p.Shop).Where(p => p.CategoryChildId == categoryChildId && p.Status == ProductStatus.Active).ToListAsync();
    }
    public async Task<List<Product>> GetListProductByVectorAsync(List<string> productIds)
    {
        return await _context.Products.Include(p => p.Shop).Where(p => productIds.Contains(p.Id) && p.Status == ProductStatus.Active    ).AsNoTracking().ToListAsync();
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        _context.Products.Update(product);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }


}