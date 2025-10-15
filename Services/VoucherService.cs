using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;
namespace PeShop.Services;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _voucherRepository;
    public VoucherService(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }
    public async Task<StatusResponse> UpdateStatusVoucherSystemAsync(string voucherSystemId, VoucherStatus status)
    {
        var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(voucherSystemId);
        if (voucherSystem == null) return new StatusResponse { Status = false };
        voucherSystem.Status = status;
        if (await _voucherRepository.UpdateVoucherSystemAsync(voucherSystem)) return new StatusResponse { Status = true };
        else return new StatusResponse { Status = false };
    }
    public async Task<StatusResponse> UpdateStatusVoucherShopAsync(string voucherShopId, VoucherStatus status)
    {
        var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(voucherShopId);
        if (voucherShop == null) return new StatusResponse { Status = false };
        voucherShop.Status = status;
        if (await _voucherRepository.UpdateVoucherShopAsync(voucherShop)) return new StatusResponse { Status = true };
        else return new StatusResponse { Status = false };
    }
}