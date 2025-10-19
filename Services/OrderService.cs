namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using System.Text.Json;
using System.Linq;
public class OrderService : IOrderService
{
    private readonly IOrderHelper _orderHelper;
    private readonly IRedisUtil _redisUtil;
    public OrderService(IOrderHelper orderHelper, IRedisUtil redisUtil)
    {
        _orderHelper = orderHelper;
        _redisUtil = redisUtil;
    }
    public async Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId)
    {
        try
        {
            // Nhóm các sản phẩm theo shop
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);
            
            // Tính tổng giá trị đơn hàng
            decimal orderTotal = itemShops.Sum(shop => shop.Total);
            
            // Tạo đơn hàng ảo
            var order = new OrderVirtualDto
            {
                OrderId = Guid.NewGuid().ToString(),
                ItemShops = itemShops,
                UserId = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            
            // Lưu vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{order.OrderId}", JsonSerializer.Serialize(order), TimeSpan.FromMinutes(30));
            
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
}