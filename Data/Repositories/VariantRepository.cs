using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Dtos.Responses;

namespace PeShop.Data.Repositories;

public class VariantRepository : IVariantRepository
{
    private readonly PeShopDbContext _context;
    public VariantRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<VariantShippingDto?> GetVariantForShippingByIdAsync(int id)
    {
        return await _context.Variants
            .Where(v => v.Id == id)
            .Select(v => new VariantShippingDto
            {
                Id = v.Id,
                Price = v.Price,
                Product = new ProductShippingDto
                {
                    Height = v.Product.Height,
                    Length = v.Product.Length,
                    Width = v.Product.Width,
                    Weight = v.Product.Weight
                }
            })
            .FirstOrDefaultAsync();
    }
}