namespace PeShop.Data.Repositories;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
using Microsoft.EntityFrameworkCore;

public class PayoutRepository : IPayoutRepository
{
    private readonly PeShopDbContext _context;
    public PayoutRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<Payout> CreatePayoutAsync(Payout payout)
    {
        await _context.Payouts.AddAsync(payout);
        await _context.SaveChangesAsync();
        return payout;
    }

    public async Task AddPayoutAsync(Payout payout)
    {
        await _context.Payouts.AddAsync(payout);
        // Không gọi SaveChangesAsync để cho phép transaction commit sau
    }

    public async Task<Payout?> GetPayoutByOrderIdAsync(string orderId)
    {
        return await _context.Payouts
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<bool> UpdatePayoutStatusAsync(string orderId, PayoutStatus status)
    {
        var payout = await _context.Payouts
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
        
        if (payout == null)
        {
            return false;
        }

        payout.Status = status;
        payout.UpdatedAt = DateTime.UtcNow;
        
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status)
    {
        return await _context.Payouts
            .Where(p => p.Status == status)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> UpdatePayoutStatusByIdAsync(int payoutId, PayoutStatus status)
    {
        var payout = await _context.Payouts
            .FirstOrDefaultAsync(p => p.Id == payoutId);
        
        if (payout == null)
        {
            return false;
        }

        payout.Status = status;
        payout.UpdatedAt = DateTime.UtcNow;
        
        // Không gọi SaveChangesAsync để cho phép transaction commit sau
        return true;
    }
}   