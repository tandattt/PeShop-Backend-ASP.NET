using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
namespace PeShop.Data.Repositories;

public class VariantValueRepository : IVariantValueRepository
{
    private readonly PeShopDbContext _context;
    public VariantValueRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<VariantValue?> GetByVariantIdAsync(int variantId)
    {
        return await _context.VariantValues.FirstOrDefaultAsync(x => x.VariantId == variantId);
    }
}