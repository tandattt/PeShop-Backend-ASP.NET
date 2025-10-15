using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;
public interface IVoucherRepository
{
    Task<VoucherSystem?> GetVoucherSystemByIdAsync(string voucherSystemId);
    Task<VoucherShop?> GetVoucherShopByIdAsync(string voucherShopId);
    Task<bool> UpdateVoucherSystemAsync(VoucherSystem voucherSystem);
    Task<bool> UpdateVoucherShopAsync(VoucherShop voucherShop);
}