using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using PeShop.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace PeShop.Data.Repositories;
public class VoucherRepository : IVoucherRepository
{
    private readonly PeShopDbContext _context;
    public VoucherRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherSystem?> GetVoucherSystemByIdAsync(string voucherSystemId)
    {
        return await _context.VoucherSystems.FirstOrDefaultAsync(v => v.Id == voucherSystemId);
    }
    public async Task<VoucherShop?> GetVoucherShopByIdAsync(string voucherShopId)
    {
        return await _context.VoucherShops.FirstOrDefaultAsync(v => v.Id == voucherShopId);
    }
    public async Task<bool> UpdateVoucherSystemAsync(VoucherSystem voucherSystem)
    {
        _context.VoucherSystems.Update(voucherSystem);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;

    }
    public async Task<bool> UpdateVoucherShopAsync(VoucherShop voucherShop)
    {
        _context.VoucherShops.Update(voucherShop);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
}