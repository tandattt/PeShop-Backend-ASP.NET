namespace PeShop.Data.Repositories;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
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
}   