using PeShop.Models.Entities;
using PeShop.Dtos.Requests;
namespace PeShop.Data.Repositories.Interfaces;
public interface IVoucherRepository
{
    Task<VoucherSystem?> GetVoucherSystemByIdAsync(string voucherSystemId);
    Task<VoucherShop?> GetVoucherShopByIdAsync(string voucherShopId);
    Task<bool> UpdateVoucherSystemAsync(VoucherSystem voucherSystem);
    Task<bool> UpdateVoucherShopAsync(VoucherShop voucherShop);
    Task<List<VoucherSystem>> GetVoucherSystemsByUserIdAsync( string userId);
    Task<List<VoucherShop>> GetUserVoucherShopsAsync( string userId);
    Task<UserVoucherShop?> GetUserVoucherShopsByVoucherShopIdAsync(string userId, string voucherShopId);
    Task<Variant?> GetVariantByIdAsync(int variantId);
    Task<List<VoucherShop>> GetVoucherShopsByShopIdAsync(string shopId);
    Task<bool> UpdateUserVoucherShopAsync(UserVoucherShop userVoucherShop);
    Task<UserVoucherSystem?> GetUserVoucherSystemByVoucherSystemIdAsync(string userId, string voucherSystemId);
    Task<bool> UpdateUserVoucherSystemAsync(UserVoucherSystem userVoucherSystem);
    Task<bool> CreateUserVoucherSystemAsync(UserVoucherSystem userVoucherSystem);
    Task<bool> CreateUserVoucherShopAsync(UserVoucherShop userVoucherShop);
    
    // Admin methods
    Task<int> GetCountVoucherSystemAsync(AGetVoucherRequest request);
    Task<List<VoucherSystem>> GetListVoucherSystemAsync(AGetVoucherRequest request);
    Task<VoucherSystem> CreateVoucherSystemAsync(VoucherSystem voucherSystem);
    Task<bool> DeleteVoucherSystemAsync(string voucherId);
}