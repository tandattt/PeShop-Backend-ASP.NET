namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Models.Enums;

public interface IPayoutRepository
{
    Task<Payout> CreatePayoutAsync(Payout payout);
    Task AddPayoutAsync(Payout payout);
    Task<Payout?> GetPayoutByOrderIdAsync(string orderId);
    Task<bool> UpdatePayoutStatusAsync(string orderId, PayoutStatus status);
}