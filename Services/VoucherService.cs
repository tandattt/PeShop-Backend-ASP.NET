using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
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
    public async Task<VoucherResponse> GetVouchersAsync(string userId)
    {
        var voucherSystems = await _voucherRepository.GetVoucherSystemsByUserIdAsync(userId);
        var voucherShops = await _voucherRepository.GetUserVoucherShopsAsync(userId);
        var voucherSystemDtos = new List<VoucherDto>();
        var voucherShopDtos = new List<VoucherDto>();
        var voucherResponses = new VoucherResponse();
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
        voucherResponses.VoucherGroups.Add(new VoucherGroupResponse
        {
            VoucherType = VoucherTypeConstant.System,
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
        voucherResponses.VoucherGroups.Add(new VoucherGroupResponse
        {
            VoucherType = VoucherTypeConstant.Shop,
            Vouchers = voucherShopDtos
        });
        return voucherResponses;
    }

    public async Task<CheckVoucherEligibilityResponse> CheckVoucherEligibilityAsync(CheckVoucherEligibilityRequest request,string userId)
    {
        var response = new CheckVoucherEligibilityResponse();

        // Calculate order total from items
        decimal orderTotal = await CalculateOrderTotalAsync(request.Items);

        // Get all available vouchers for the user
        var voucherSystems = await _voucherRepository.GetVoucherSystemsByUserIdAsync(userId);
        var voucherShops = await _voucherRepository.GetUserVoucherShopsAsync(userId);

        var allVouchers = new List<(string Id, string Name, string Code, decimal? DiscountValue, decimal? MaxDiscountAmount, decimal? MinOrderValue, VoucherValueType? Type, string VoucherType, DateTime? StartTime, DateTime? EndTime, uint? LimitForUser, uint? UsedCount)>();

        // Add system vouchers
        foreach (var vs in voucherSystems)
        {
            var usedCount = vs.UserVoucherSystems?.Where(uv => uv.UserId == userId).Sum(uv => uv.UsedCount) ?? 0;
            allVouchers.Add((vs.Id, vs.Name ?? "", vs.Code ?? "", vs.DiscountValue, vs.MaxdiscountAmount, vs.MiniumOrderValue, vs.Type, VoucherTypeConstant.System, vs.StartTime, vs.EndTime, vs.LimitForUser, (uint)usedCount));
        }

        // Add shop vouchers
        foreach (var vs in voucherShops)
        {
            var usedCount = vs.UserVoucherShops?.Where(uv => uv.UserId == userId).Sum(uv => uv.UsedCount) ?? 0;
            allVouchers.Add((vs.Id, vs.Name ?? "", vs.Code ?? "", vs.DiscountValue, vs.MaxdiscountAmount, vs.MinimumOrderValue, vs.Type, VoucherTypeConstant.Shop, vs.StartTime, vs.EndTime, vs.LimitForUser, (uint)usedCount));
        }

        // No filtering needed - check all available vouchers

        // Group vouchers by type
        var systemVouchers = new List<VoucherEligibilityItem>();
        var shopVouchers = new List<VoucherEligibilityItem>();

        foreach (var voucher in allVouchers)
        {
            var (voucherId, voucherName, voucherCode, isEligible, reasons, discountAmount, finalOrderTotal) = CheckSingleVoucherEligibility(voucher, orderTotal, userId);
            
            var voucherDto = new VoucherDto
            {
                Id = voucherId,
                Name = voucherName,
                Code = voucherCode,
                DiscountValue = voucher.DiscountValue ?? 0,
                MaxdiscountAmount = voucher.MaxDiscountAmount ?? 0,
                MiniumOrderValue = voucher.MinOrderValue ?? 0,
                StartTime = voucher.StartTime ?? DateTime.MinValue,
                EndTime = voucher.EndTime ?? DateTime.MinValue,
                ValueType = voucher.Type,
                ValueTypeName = EnumExtensions.ToVietnameseString(voucher.Type)
            };

            var voucherItem = new VoucherEligibilityItem
            {
                IsAllowed = isEligible,
                Reason = string.Join("; ", reasons),
                Voucher = voucherDto
            };

            if (voucher.VoucherType == VoucherTypeConstant.System)
            {
                systemVouchers.Add(voucherItem);
            }
            else
            {
                shopVouchers.Add(voucherItem);
            }
        }

        // Add voucher type groups
        if (systemVouchers.Any())
        {
            response.VoucherTypes.Add(new VoucherTypeGroup
            {
                VoucherType = VoucherTypeConstant.System,
                Vouchers = systemVouchers
            });
        }

        if (shopVouchers.Any())
        {
            response.VoucherTypes.Add(new VoucherTypeGroup
            {
                VoucherType = VoucherTypeConstant.Shop,
                Vouchers = shopVouchers
            });
        }

        // Find best voucher (highest discount among all eligible vouchers)
        var allEligibleVouchers = systemVouchers.Concat(shopVouchers).Where(v => v.IsAllowed).ToList();
        if (allEligibleVouchers.Any())
        {
            var bestVoucher = allEligibleVouchers.OrderByDescending(v => v.Voucher.DiscountValue).First();
            // Set best voucher ID in the appropriate group
            var bestVoucherGroup = response.VoucherTypes.FirstOrDefault(g => g.Vouchers.Contains(bestVoucher));
            if (bestVoucherGroup != null)
            {
                bestVoucherGroup.BestVoucherId = bestVoucher.Voucher.Id;
            }
        }

        return response;
    }

    private async Task<decimal> CalculateOrderTotalAsync(List<OrderItemRequest> items)
    {
        decimal total = 0;
        
        foreach (var item in items)
        {
            decimal price = 0;
            
            if (!string.IsNullOrEmpty(item.VariantId))
            {
                // Lấy giá từ variant
                var variant = await _voucherRepository.GetVariantByIdAsync(item.VariantId);
                if (variant != null)
                {
                    price = variant.Price ?? 0;
                }
            }
            else
            {
                // Lấy giá từ product
                var product = await _voucherRepository.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    price = product.Price ?? 0;
                }
            }
            
            total += price * item.Quantity;
        }
        
        return total;
    }

    private (string VoucherId, string VoucherName, string VoucherCode, bool IsEligible, List<string> Reasons, decimal DiscountAmount, decimal FinalOrderTotal) CheckSingleVoucherEligibility(
        (string Id, string Name, string Code, decimal? DiscountValue, decimal? MaxDiscountAmount, decimal? MinOrderValue, VoucherValueType? Type, string VoucherType, DateTime? StartTime, DateTime? EndTime, uint? LimitForUser, uint? UsedCount) voucher,
        decimal orderTotal,
        string? userId)
    {
        var isEligible = true;

        var reasons = new List<string>();

        // Check if voucher is active (status check would be done in repository)
        
        // Check minimum order value
        if (voucher.MinOrderValue.HasValue && orderTotal < voucher.MinOrderValue.Value)
        {
            isEligible = false;
            reasons.Add($"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderValue.Value:C}");
        }

        // Check time validity
        var now = DateTime.Now;
        if (voucher.StartTime.HasValue && now < voucher.StartTime.Value)
        {
            isEligible = false;
            reasons.Add($"Voucher chưa có hiệu lực (bắt đầu từ {voucher.StartTime.Value:dd/MM/yyyy HH:mm})");
        }

        if (voucher.EndTime.HasValue && now > voucher.EndTime.Value)
        {
            isEligible = false;
            reasons.Add($"Voucher đã hết hạn (kết thúc lúc {voucher.EndTime.Value:dd/MM/yyyy HH:mm})");
        }

        // Check usage limit
        if (voucher.LimitForUser.HasValue && voucher.UsedCount >= voucher.LimitForUser.Value)
        {
            isEligible = false;
            reasons.Add($"Đã sử dụng hết số lần cho phép ({voucher.UsedCount}/{voucher.LimitForUser})");
        }

        // Calculate discount amount if eligible
        decimal discountAmount = 0;
        decimal finalOrderTotal = orderTotal;
        
        if (isEligible && voucher.DiscountValue.HasValue)
        {
            if (voucher.Type == VoucherValueType.Percentage)
            {
                discountAmount = orderTotal * (voucher.DiscountValue.Value / 100);
            }
            else if (voucher.Type == VoucherValueType.FixedAmount)
            {
                discountAmount = voucher.DiscountValue.Value;
            }

            // Apply maximum discount limit
            if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
            {
                discountAmount = voucher.MaxDiscountAmount.Value;
            }

            finalOrderTotal = Math.Max(0, orderTotal - discountAmount);
        }

        return (voucher.Id, voucher.Name, voucher.Code, isEligible, reasons, discountAmount, finalOrderTotal);
    }
}