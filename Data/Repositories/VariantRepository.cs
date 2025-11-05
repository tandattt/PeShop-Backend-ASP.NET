using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Dtos.Responses;
using PeShop.Models.Enums;

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
            .Where(v => v.Id == id && v.Status == VariantStatus.Show)
            .Select(v => new VariantShippingDto
            {
                Id = v.Id,
                Price = v.Price,
                Status = v.Status ?? VariantStatus.Show,
                Product = new ProductShippingDto
                {
                    Height = v.Product!.Height,
                    Length = v.Product!.Length,
                    Width = v.Product!.Width,
                    Weight = v.Product!.Weight
                }
            })
            .FirstOrDefaultAsync();
    }
    public async Task<Variant?> GetVariantByIdAsync(string id)
    {
        return await _context.Variants
            .Where(v => v.Id == int.Parse(id) && v.Status == VariantStatus.Show)
            .FirstOrDefaultAsync();
    }
    public async Task<bool> UpdateVariantAsync(Variant variant)
    {
        _context.Variants.Update(variant);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
}