namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using System.Text.Json;

using System.Linq;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
public class OrderService : IOrderService
{
    private readonly IOrderHelper _orderHelper;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IRedisUtil _redisUtil;
    public OrderService(IOrderHelper orderHelper, IRedisUtil redisUtil, IVoucherRepository voucherRepository)
    {
        _orderHelper = orderHelper;
        _redisUtil = redisUtil;
        _voucherRepository = voucherRepository;
    }
    public async Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId)
    {
        try
        {
            // Nhóm các sản phẩm theo shop
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Tính tổng giá trị đơn hàng
            decimal orderTotal = itemShops.Sum(shop => shop.PriceOriginal);

            // Tạo đơn hàng ảo
            var order = new OrderVirtualDto
            {
                OrderId = Guid.NewGuid().ToString(),
                ItemShops = itemShops,
                OrderTotal = orderTotal,
                AmountTotal = orderTotal,
                UserId = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Lưu vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{order.OrderId}", JsonSerializer.Serialize(order));

            if (isSaved)
            {
                return new CreateVirtualOrderResponse
                {
                    Status = true,
                    Order = order,
                    Message = "Đơn hàng đã được tạo thành công"
                };
            }
            else
            {
                return new CreateVirtualOrderResponse
                {
                    Status = false,
                    Message = "Lỗi khi lưu đơn hàng vào Redis"
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateVirtualOrderResponse
            {
                Status = false,
                Message = $"Lỗi khi tạo đơn hàng: {ex.Message}"
            };
        }
    }
    public async Task<CreateVirtualOrderResponse> CalclulateOrderTotal(string orderId, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");

        foreach (var itemShop in order.ItemShops)
        {
            itemShop.PriceAfterVoucher = itemShop.PriceOriginal - 0;
            if (itemShop.VoucherId == null) continue;
            
            var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(itemShop.VoucherId);
            if (voucherShop.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0) > (voucherShop.MaxdiscountAmount ?? 0) ? (voucherShop.MaxdiscountAmount ?? 0) : itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0);
                itemShop.PriceAfterVoucher = itemShop.PriceOriginal - discountAmount;
            }
            else if (voucherShop.Type == VoucherValueType.FixedAmount)
            {
                itemShop.PriceAfterVoucher = itemShop.PriceOriginal - voucherShop.DiscountValue ?? 0;
            }
        }

        if (order.VoucherSystemId != null)
        {
            var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(order.VoucherSystemId);
            if (voucherSystem.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = order.OrderTotal * (voucherSystem.DiscountValue ?? 0) > (voucherSystem.MaxdiscountAmount ?? 0) ? (voucherSystem.MaxdiscountAmount ?? 0) : order.OrderTotal * (voucherSystem.DiscountValue ?? 0);
                order.OrderTotal = order.OrderTotal - discountAmount;
            }
            else if (voucherSystem.Type == VoucherValueType.FixedAmount)
            {
                order.OrderTotal = order.OrderTotal - voucherSystem.DiscountValue ?? 0;
            }
        }
        order.AmountTotal = order.OrderTotal +  order.FeeShippingTotal;
        await _redisUtil.SetAsync($"calculated_order_{userId}_{orderId}", JsonSerializer.Serialize(order));
        return new CreateVirtualOrderResponse
        {
            Status = true,
            Order = order,
            Message = "Đơn hàng đã được tính toán thành công"
        };
    }
}