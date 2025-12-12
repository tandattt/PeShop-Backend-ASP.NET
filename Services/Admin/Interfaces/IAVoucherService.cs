using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Models.Enums;

namespace PeShop.Services.Admin.Interfaces;

public interface IAVoucherService
{
    Task<PaginationResponse<AVoucherResponse>> GetVouchersAsync(AGetVoucherRequest request);
    Task<StatusResponse<AVoucherResponse>> CreateAsync(ACreateVoucherRequest request, string userId);
    Task<StatusResponse<AVoucherResponse>> UpdateAsync(string voucherId, AUpdateVoucherRequest request, string userId);
    Task<StatusResponse> DeleteAsync(string voucherId);
    Task<StatusResponse> SetExpiredAsync(string voucherId);
    Task<StatusResponse> UpdateVoucherStatusJobAsync(string voucherId, VoucherStatus status);
}

