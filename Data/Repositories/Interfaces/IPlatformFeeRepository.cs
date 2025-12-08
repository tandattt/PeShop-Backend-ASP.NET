namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IPlatformFeeRepository
{
    Task<decimal> GetPlatformFeeByCategoryIdAsync(string categoryId);
    Task<PlatformFee> CreateAsync(PlatformFee platformFee);
    Task<PlatformFee?> GetByIdAsync(uint id);
    Task<List<PlatformFee>> GetAllAsync();
    Task<(List<PlatformFee> Data, int TotalCount)> GetAllAsync(int page, int pageSize, string? categoryId);
    Task<List<PlatformFee>> GetByCategoryIdAsync(string categoryId);
    Task<(List<PlatformFee> Data, int TotalCount)> GetByCategoryIdAsync(string categoryId, int page, int pageSize);
    Task<PlatformFee> UpdateAsync(PlatformFee platformFee);
    Task DeactivateOthersByCategoryIdAsync(string categoryId, uint excludeId);
}