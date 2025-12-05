namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IPlatformFeeRepository
{
    Task<decimal> GetPlatformFeeByCategoryIdAsync(string categoryId);
    Task<PlatformFee> CreateAsync(PlatformFee platformFee);
    Task<PlatformFee?> GetByIdAsync(uint id);
    Task<List<PlatformFee>> GetAllAsync();
    Task<List<PlatformFee>> GetByCategoryIdAsync(string categoryId);
    Task<PlatformFee> UpdateAsync(PlatformFee platformFee);
    Task DeactivateOthersByCategoryIdAsync(string categoryId, uint excludeId);
}