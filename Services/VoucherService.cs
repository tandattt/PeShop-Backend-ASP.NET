using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;
using PeShop.Constants;
using PeShop.Dtos.Shared;
using PeShop.Extensions;
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
    public async Task<List<VoucherResponse>> GetVouchersAsync(string userId)
    {
        var voucherSystems = await _voucherRepository.GetVoucherSystemsByUserIdAsync(userId);
        var voucherShops = await _voucherRepository.GetUserVoucherShopsAsync(userId);
        var voucherSystemDtos = new List<VoucherDto>();
        var voucherShopDtos = new List<VoucherDto>();
        var voucherResponses = new List<VoucherResponse>();
        foreach (var voucherSystem in voucherSystems)
        {
            
            voucherSystemDtos.Add(new VoucherDto
            {
                Id = voucherSystem.Id,
                Name = voucherSystem.Name ?? string.Empty,
                Code = voucherSystem.Code ?? string.Empty,
                Quantity = (uint)(voucherSystem.LimitForUser - voucherSystem.UserVoucherSystems.Sum(uv => uv.UsedCount ?? 0) ?? voucherSystem.LimitForUser ?? 0),
                DiscountValue = voucherSystem.DiscountValue ?? 0,
                MaxdiscountAmount = voucherSystem.MaxdiscountAmount ?? 0,
                MiniumOrderValue = voucherSystem.MiniumOrderValue ?? 0,
                StartTime = voucherSystem.StartTime ?? DateTime.MinValue,
                EndTime = voucherSystem.EndTime ?? DateTime.MinValue    ,
                // Type = VoucherTypeConstant.System,
                ValueType = voucherSystem.Type ?? null,
                ValueTypeName = EnumExtensions.ToVietnameseString(voucherSystem.Type) ?? null
            });
        }
        voucherResponses.Add(new VoucherResponse
        {
            Type = VoucherTypeConstant.System,
            Vouchers = voucherSystemDtos
        });
        foreach (var voucherShop in voucherShops)
        {
            voucherShopDtos.Add(new VoucherDto
            {
                Id = voucherShop.Id,
                Name = voucherShop.Name ?? string.Empty,
                Code = voucherShop.Code ?? string.Empty,
                Quantity = (uint)(voucherShop.LimitForUser - voucherShop.UserVoucherShops.Sum(uv => uv.UsedCount ?? 0) ?? voucherShop.LimitForUser ?? 0),
                DiscountValue = voucherShop.DiscountValue ?? 0,
                MaxdiscountAmount = voucherShop.MaxdiscountAmount ?? 0,
                MiniumOrderValue = voucherShop.MinimumOrderValue ?? 0,
                StartTime = voucherShop.StartTime ?? DateTime.MinValue,
                EndTime = voucherShop.EndTime ?? DateTime.MinValue,
                // Type = VoucherTypeConstant.Shop,
                ValueType = voucherShop.Type ?? null,
                ValueTypeName = EnumExtensions.ToVietnameseString(voucherShop.Type) ?? null
            });
        }
        voucherResponses.Add(new VoucherResponse
        {
            Type = VoucherTypeConstant.Shop,
            Vouchers = voucherShopDtos
        });
        return voucherResponses;
    }
}