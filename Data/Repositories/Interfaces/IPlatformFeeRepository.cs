namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IPlatformFeeRepository
{
    Task<decimal> GetPlatformFeeByCategoryIdAsync(string categoryId);
}