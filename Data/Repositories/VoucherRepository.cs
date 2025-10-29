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
    public async Task<List<VoucherSystem>> GetVoucherSystemsByUserIdAsync(string userId)
    {
        return await _context.VoucherSystems.Include(v => v.UserVoucherSystems)
        .Where(v => v.Status == VoucherStatus.Active || (v.Status == VoucherStatus.Active && v.UserVoucherSystems.Any(uv => uv.UserId == userId && uv.UsedCount < v.LimitForUser))).ToListAsync();
    }
    public async Task<List<VoucherShop>> GetUserVoucherShopsAsync(string userId)
    {
        return await _context.VoucherShops
        .Include(v => v.UserVoucherShops)
        .Where(v => v.Status == VoucherStatus.Active && v.UserVoucherShops.Any(uv => uv.UserId == userId && uv.UsedCount < v.LimitForUser))
        .ToListAsync();
    }

    public async Task<Variant?> GetVariantByIdAsync(int variantId)
    {
        return await _context.Variants.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == variantId);
    }
    public async Task<List<VoucherShop>> GetVoucherShopsByShopIdAsync(string shopId)
    {
        return await _context.VoucherShops
        .Include(v => v.UserVoucherShops)
        .Where(v => v.Status == VoucherStatus.Active && v.ShopId == shopId)
        .ToListAsync();
    }
    public async Task<UserVoucherShop?> GetUserVoucherShopsByVoucherShopIdAsync(string userId, string voucherShopId)
    {
        return await _context.UserVoucherShops
        .FirstOrDefaultAsync(v => v.UserId == userId && v.VoucherShopId == voucherShopId);
    }
    public async Task<bool> UpdateUserVoucherShopAsync(UserVoucherShop userVoucherShop)
    {
        _context.UserVoucherShops.Update(userVoucherShop);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
    public async Task<bool> CreateUserVoucherShopAsync(UserVoucherShop userVoucherShop)
    {
        _context.UserVoucherShops.Add(userVoucherShop);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }

    public async Task<UserVoucherSystem?> GetUserVoucherSystemByVoucherSystemIdAsync(string userId, string voucherSystemId)
    {
        return await _context.UserVoucherSystems
            .FirstOrDefaultAsync(u => u.UserId == userId && u.VoucherSystemId == voucherSystemId);
    }

    public async Task<bool> UpdateUserVoucherSystemAsync(UserVoucherSystem userVoucherSystem)
    {
        _context.UserVoucherSystems.Update(userVoucherSystem);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }

    public async Task<bool> CreateUserVoucherSystemAsync(UserVoucherSystem userVoucherSystem)
    {
        _context.UserVoucherSystems.Add(userVoucherSystem);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }

}