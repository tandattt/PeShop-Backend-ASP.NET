using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Models.Enums;
namespace PeShop.Services.Interfaces;

public interface IVoucherService
{
    Task<StatusResponse> UpdateStatusVoucherSystemAsync(string voucherSystemId, VoucherStatus status);
    Task<StatusResponse> UpdateStatusVoucherShopAsync(string voucherShopId, VoucherStatus status);
    Task<VoucherResponse> GetVouchersAsync(string userId);
    Task<CheckVoucherEligibilityResponse> CheckVoucherEligibilityAsync(string userId,string orderId);
    Task<StatusResponse> ApplyVoucherSystemAsync(string userId, ApplyVoucherSystemRequest request);
    Task<StatusResponse> ApplyVoucherShopAsync(string userId, ApplyVoucherShopRequest request);
}