namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IPayoutRepository
{
    Task<Payout> CreatePayoutAsync(Payout payout);
    Task AddPayoutAsync(Payout payout);
}