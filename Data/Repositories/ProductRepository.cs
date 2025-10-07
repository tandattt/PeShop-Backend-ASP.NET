using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;

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

    public async Task<Product> GetProductForShippingByIdAsync(string productId)
    {
        return await _context.Products
            .Select(p => new Product
            {
                Price = p.Price,
                Height = p.Height,
                Length = p.Length,
                Width = p.Width,
                Weight = p.Weight,
            })
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}