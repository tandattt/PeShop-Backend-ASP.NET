using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Dtos.Responses;

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
            .Include(p => p.Variants)
                .ThenInclude(v => v.VariantValues)
                    .ThenInclude(vv => vv.PropertyValue)
                        .ThenInclude(pv => pv.PropertyProduct)
            .FirstOrDefaultAsync(p => p.Id == productId);
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
}