using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using PeShop.Models.Enums;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Requests;
using PeShop.Constants;
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
        return await _context.VoucherSystems.AsNoTracking().FirstOrDefaultAsync(v => v.Id == voucherSystemId);
    }
    public async Task<VoucherShop?> GetVoucherShopByIdAsync(string voucherShopId)
    {
        return await _context.VoucherShops.AsNoTracking().FirstOrDefaultAsync(v => v.Id == voucherShopId);
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
        .Where(v => v.Status == VoucherStatus.Active || (v.Status == VoucherStatus.Active && v.UserVoucherSystems.Any(uv => uv.UserId == userId && uv.UsedCount < v.LimitForUser)))
        .AsNoTracking()
        .ToListAsync();
    }
    public async Task<List<VoucherShop>> GetUserVoucherShopsAsync(string userId)
    {
        return await _context.VoucherShops
        .Include(v => v.UserVoucherShops)
        .Where(v => v.Status == VoucherStatus.Active && v.UserVoucherShops.Any(uv => uv.UserId == userId && uv.UsedCount < v.LimitForUser))
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<Variant?> GetVariantByIdAsync(int variantId)
    {
        return await _context.Variants.Include(v => v.Product).AsNoTracking().FirstOrDefaultAsync(v => v.Id == variantId);
    }
    public async Task<List<VoucherShop>> GetVoucherShopsByShopIdAsync(string shopId)
    {
        return await _context.VoucherShops
        .Include(v => v.UserVoucherShops)
        .Where(v => v.Status == VoucherStatus.Active && v.ShopId == shopId)
        .AsNoTracking()
        .ToListAsync();
    }
    public async Task<UserVoucherShop?> GetUserVoucherShopsByVoucherShopIdAsync(string userId, string voucherShopId)
    {
        return await _context.UserVoucherShops
        .AsNoTracking()
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
            .AsNoTracking()
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

    // Admin methods
    public async Task<int> GetCountVoucherSystemAsync(AGetVoucherRequest request)
    {
        var query = BuildQuery(request);
        return await query.CountAsync();
    }

    public async Task<List<VoucherSystem>> GetListVoucherSystemAsync(AGetVoucherRequest request)
    {
        var query = BuildQuery(request);
        
        // Sort theo CreatedAt
        if (request.SortOrder?.ToLower() == SortOrderConstants.Oldest)
        {
            query = query.OrderBy(v => v.CreatedAt);
        }
        else // Mặc định là newest
        {
            query = query.OrderByDescending(v => v.CreatedAt);
        }
        
        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        return await query
            .AsNoTracking()
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();
    }

    private IQueryable<VoucherSystem> BuildQuery(AGetVoucherRequest request)
    {
        var query = _context.VoucherSystems.AsQueryable();
        
        // Filter theo Code
        if (!string.IsNullOrEmpty(request.Code))
        {
            query = query.Where(v => v.Code != null && v.Code.Contains(request.Code));
        }
        
        // Filter theo Type
        if (request.Type.HasValue)
        {
            query = query.Where(v => v.Type == request.Type.Value);
        }
        
        // Filter theo Status
        if (request.Status.HasValue)
        {
            query = query.Where(v => v.Status == request.Status.Value);
        }
        
        // Filter theo DateFrom
        if (request.DateFrom.HasValue)
        {
            query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt >= request.DateFrom.Value);
        }
        
        // Filter theo DateTo
        if (request.DateTo.HasValue)
        {
            var dateToEndOfDay = request.DateTo.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt <= dateToEndOfDay);
        }
        
        return query;
    }

    public async Task<VoucherSystem> CreateVoucherSystemAsync(VoucherSystem voucherSystem)
    {
        _context.VoucherSystems.Add(voucherSystem);
        await _context.SaveChangesAsync();
        return voucherSystem;
    }

    public async Task<bool> DeleteVoucherSystemAsync(string voucherId)
    {
        var voucher = await _context.VoucherSystems.FirstOrDefaultAsync(v => v.Id == voucherId);
        if (voucher == null)
        {
            return false;
        }

        _context.VoucherSystems.Remove(voucher);
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Foreign key constraint violation
            return false;
        }
    }

}