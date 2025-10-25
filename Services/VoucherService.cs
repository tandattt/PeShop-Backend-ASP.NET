using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Constants;
using PeShop.Dtos.Shared;
using PeShop.Extensions;
using PeShop.Interfaces;
using PeShop.Exceptions;
using System.Text.Json;
namespace PeShop.Services;

public class VoucherService : IVoucherService
{
    private readonly IRedisUtil _redisUtil;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderHelper _orderHelper;
    public VoucherService(IVoucherRepository voucherRepository, IOrderHelper orderHelper, IRedisUtil redisUtil)
    {
        _redisUtil = redisUtil;
        _voucherRepository = voucherRepository;
        _orderHelper = orderHelper;
        _redisUtil = redisUtil;
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
                ValueType = voucherSystem.Type,
                ValueTypeName = EnumExtensions.ToVietnameseString(voucherSystem.Type)
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
                ValueType = voucherShop.Type,
                ValueTypeName = EnumExtensions.ToVietnameseString(voucherShop.Type)
            });
        }
        voucherResponses.VoucherGroups.Add(new VoucherGroupResponse
        {
            VoucherType = VoucherTypeConstant.Shop,
            Vouchers = voucherShopDtos
        });
        return voucherResponses; 
    }

    public async Task<CheckVoucherEligibilityResponse> CheckVoucherEligibilityAsync(string userId,string orderId)
    {
        var response = new CheckVoucherEligibilityResponse();
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");
        if (order == null) {
            throw new BadRequestException("Order not found");
        }
        // Calculate order total from items
        decimal orderTotal = order.ItemShops.Sum(shop => shop.PriceOriginal);

        // Get all available vouchers for the user
        var voucherSystems = await _voucherRepository.GetVoucherSystemsByUserIdAsync(userId);
        
        // Get shop vouchers for each shop in the order
        var allVouchers = new List<(string Id, string Name, string Code, decimal? DiscountValue, decimal? MaxDiscountAmount, decimal? MinOrderValue, VoucherValueType? Type, string VoucherType, DateTime? StartTime, DateTime? EndTime, uint? LimitForUser, uint? UsedCount, string ShopId)>();

        // Add system vouchers
        foreach (var vs in voucherSystems)
        {
            var usedCount = vs.UserVoucherSystems?.Where(uv => uv.UserId == userId).Sum(uv => uv.UsedCount) ?? 0;
            allVouchers.Add((vs.Id, vs.Name ?? "", vs.Code ?? "", vs.DiscountValue, vs.MaxdiscountAmount, vs.MiniumOrderValue, vs.Type, VoucherTypeConstant.System, vs.StartTime, vs.EndTime, vs.LimitForUser, (uint)usedCount, ""));
        }

        // Add shop vouchers for each shop in the order
        foreach (var shop in order.ItemShops)
        {
            var voucherShops = await _voucherRepository.GetVoucherShopsByShopIdAsync(shop.ShopId);
            foreach (var vs in voucherShops)
            {
                var usedCount = vs.UserVoucherShops?.Where(uv => uv.UserId == userId).Sum(uv => uv.UsedCount) ?? 0;
                allVouchers.Add((vs.Id, vs.Name ?? "", vs.Code ?? "", vs.DiscountValue, vs.MaxdiscountAmount, vs.MinimumOrderValue, vs.Type, VoucherTypeConstant.Shop, vs.StartTime, vs.EndTime, vs.LimitForUser, (uint)usedCount, shop.ShopId));
            }
        }

        // No filtering needed - check all available vouchers

        // Group vouchers by type
        var systemVouchers = new List<VoucherEligibilityItem>();
        var shopVoucherGroups = new Dictionary<string, ShopVoucherGroup>();

        foreach (var voucher in allVouchers)
        {
            var (voucherId, voucherName, voucherCode, isEligible, reasons, discountAmount, finalOrderTotal) = CheckSingleVoucherEligibility(voucher, orderTotal, userId, order.ItemShops);
            
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
                ValueType = voucher.Type ?? VoucherValueType.Percentage,
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
                // Group shop vouchers by shop
                if (!shopVoucherGroups.ContainsKey(voucher.ShopId))
                {
                    var shop = order.ItemShops.FirstOrDefault(s => s.ShopId == voucher.ShopId);
                    shopVoucherGroups[voucher.ShopId] = new ShopVoucherGroup
                    {
                        ShopId = voucher.ShopId,
                        ShopName = shop?.ShopName ?? "Unknown Shop",
                        Vouchers = new List<VoucherEligibilityItem>()
                    };
                }
                shopVoucherGroups[voucher.ShopId].Vouchers.Add(voucherItem);
            }
        }

        // Add system vouchers
        if (systemVouchers.Any())
        {
            var eligibleSystemVouchers = systemVouchers.Where(v => v.IsAllowed).ToList();
            var bestSystemVoucher = eligibleSystemVouchers.Any() ? 
                eligibleSystemVouchers.OrderByDescending(v => v.Voucher.DiscountValue).First() : null;

            response.SystemVouchers = new VoucherTypeGroup
            {
                VoucherType = VoucherTypeConstant.System,
                Vouchers = systemVouchers,
                BestVoucherId = bestSystemVoucher?.Voucher.Id
            };
        }

        // Add shop voucher groups
        foreach (var shopGroup in shopVoucherGroups.Values)
        {
            var eligibleShopVouchers = shopGroup.Vouchers.Where(v => v.IsAllowed).ToList();
            var bestShopVoucher = eligibleShopVouchers.Any() ? 
                eligibleShopVouchers.OrderByDescending(v => v.Voucher.DiscountValue).First() : null;

            shopGroup.BestVoucherId = bestShopVoucher?.Voucher.Id;
            response.ShopVouchers.Add(shopGroup);
        }
        await _redisUtil.SetAsync($"voucher_eligibility_{userId}_{orderId}", JsonSerializer.Serialize(response));
        return response;
    }

    public async Task<StatusResponse> ApplyVoucherSystemAsync(string userId, ApplyVoucherSystemRequest request)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
        var vouchers = await _redisUtil.GetAsync<CheckVoucherEligibilityResponse>($"voucher_eligibility_{userId}_{request.OrderId}");
        
        if (order == null)
        {
            throw new BadRequestException("Order not found");
        }

        if (vouchers?.SystemVouchers?.Vouchers?.FirstOrDefault(vv => vv.Voucher.Id == request.VoucherId) == null)
        {
            throw new BadRequestException("System voucher not found or not eligible");
        }
        var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(request.VoucherId);
        if (voucherSystem.Type == VoucherValueType.Percentage)
        {
            order.VoucherSystemValue = order.OrderTotal * (voucherSystem.DiscountValue ?? 0) > (voucherSystem.MaxdiscountAmount ?? 0) ? (voucherSystem.MaxdiscountAmount ?? 0) : order.OrderTotal * (voucherSystem.DiscountValue ?? 0);
        }
        else if (voucherSystem.Type == VoucherValueType.FixedAmount)
        {
            order.VoucherSystemValue = voucherSystem.DiscountValue ?? 0;
        }
        order.VoucherSystemId = request.VoucherId;
        order.VoucherSystemName = voucherSystem.Name;
        await _redisUtil.SetAsync($"order_{userId}_{request.OrderId}", JsonSerializer.Serialize(order));
        
        return new StatusResponse { Status = true, Message = "System voucher applied successfully" };
    }

    public async Task<StatusResponse> ApplyVoucherShopAsync(string userId, ApplyVoucherShopRequest request)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
        var vouchers = await _redisUtil.GetAsync<CheckVoucherEligibilityResponse>($"voucher_eligibility_{userId}_{request.OrderId}");
        
        if (order == null)
        {
            throw new BadRequestException("Order not found");
        }

        // Find the specific shop in the order
        var targetShop = order.ItemShops.FirstOrDefault(s => s.ShopId == request.ShopId);
        if (targetShop == null)
        {
            throw new BadRequestException("Shop not found in order");
        }

        // Check if the voucher exists for this specific shop
        var shopVoucherGroup = vouchers?.ShopVouchers?.FirstOrDefault(s => s.ShopId == request.ShopId);
        if (shopVoucherGroup?.Vouchers?.FirstOrDefault(vv => vv.Voucher.Id == request.VoucherId) == null)
        {
            throw new BadRequestException("Shop voucher not found or not eligible for this shop");
        }
        var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(request.VoucherId);
        if (voucherShop.Type == VoucherValueType.Percentage)
        {
            targetShop.VoucherValue = targetShop.PriceOriginal * (voucherShop.DiscountValue ?? 0) > (voucherShop.MaxdiscountAmount ?? 0) ? (voucherShop.MaxdiscountAmount ?? 0) : targetShop.PriceOriginal * (voucherShop.DiscountValue ?? 0);
        }
        else if (voucherShop.Type == VoucherValueType.FixedAmount)
        {
            targetShop.VoucherValue = voucherShop.DiscountValue ?? 0;
        }

        // Apply voucher to the specific shop
        targetShop.VoucherId = request.VoucherId;
        targetShop.VoucherName = voucherShop.Name;
        await _redisUtil.SetAsync($"order_{userId}_{request.OrderId}", JsonSerializer.Serialize(order));
        
        return new StatusResponse { Status = true, Message = "Shop voucher applied successfully" };
    }

    private (string VoucherId, string VoucherName, string VoucherCode, bool IsEligible, List<string> Reasons, decimal DiscountAmount, decimal FinalOrderTotal) CheckSingleVoucherEligibility(
        (string Id, string Name, string Code, decimal? DiscountValue, decimal? MaxDiscountAmount, decimal? MinOrderValue, VoucherValueType? Type, string VoucherType, DateTime? StartTime, DateTime? EndTime, uint? LimitForUser, uint? UsedCount, string ShopId) voucher,
        decimal orderTotal,
        string? userId,
        List<ItemShop> itemShops)
    {
        var isEligible = true;

        var reasons = new List<string>();

        // Check if voucher is active (status check would be done in repository)
        
        // Check minimum order value
        decimal relevantOrderTotal = orderTotal;
        if (voucher.VoucherType == VoucherTypeConstant.Shop && !string.IsNullOrEmpty(voucher.ShopId))
        {
            // For shop vouchers, check against the specific shop's total
            var shop = itemShops.FirstOrDefault(s => s.ShopId == voucher.ShopId);
            if (shop == null)
            {
                isEligible = false;
                reasons.Add("Shop không tồn tại trong đơn hàng");
                return (voucher.Id, voucher.Name, voucher.Code, isEligible, reasons, 0, orderTotal);
            }
            relevantOrderTotal = shop.PriceOriginal;
        }
        
        if (voucher.MinOrderValue.HasValue && relevantOrderTotal < voucher.MinOrderValue.Value)
        {
            isEligible = false;
            if (voucher.VoucherType == VoucherTypeConstant.Shop)
            {
                reasons.Add($"Đơn hàng từ shop này phải có giá trị tối thiểu {voucher.MinOrderValue.Value:C}");
            }
            else
            {
                reasons.Add($"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderValue.Value:C}");
            }
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
                discountAmount = relevantOrderTotal * (voucher.DiscountValue.Value / 100);
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

            // For shop vouchers, only discount the specific shop's total
            if (voucher.VoucherType == VoucherTypeConstant.Shop && !string.IsNullOrEmpty(voucher.ShopId))
            {
                finalOrderTotal = orderTotal; // Total order remains the same, only shop total is discounted
            }
            else
            {
                finalOrderTotal = Math.Max(0, orderTotal - discountAmount);
            }
        }

        return (voucher.Id, voucher.Name, voucher.Code, isEligible, reasons, discountAmount, finalOrderTotal);
    }
}