using PeShop.Dtos.Responses;
using PeShop.Models.Enums;
namespace PeShop.Services.Interfaces;

public interface IVoucherService
{
    Task<StatusResponse> UpdateStatusVoucherSystemAsync(string voucherSystemId, VoucherStatus status);
    Task<StatusResponse> UpdateStatusVoucherShopAsync(string voucherShopId, VoucherStatus status);
    Task<List<VoucherResponse>> GetVouchersAsync(string userId);
}