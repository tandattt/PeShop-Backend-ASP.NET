using PeShop.Models.Entities;
using PeShop.Models.Enums;

namespace PeShop.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    // Task<Order> GetOrderByIdAsync(string orderId);
    Task<Order> CreateOrderAsync(Order order);
    // Task<Order> UpdateOrderAsync(Order order);
    // Task<Order> DeleteOrderAsync(string orderId);
    Task<OrderVoucher> CreateOrderVoucherAsync(OrderVoucher orderVoucher);
    Task<Order> GetOrderByIdAsync(string orderId, string userId);
    Task<List<Order>> GetOrdersByIdsAsync(List<string> orderIds, string userId);
    Task<bool> UpdatePaymentStatusInOrderAsync(Order order);
    Task<List<Order>> GetOrderByUserIdAsync(string userId);
    Task<Order?> GetOrderDetailAsync(string orderId, string userId);
    
    // GHN Webhook methods
    Task<Order?> GetOrderByOrderCodeAsync(string orderCode);
    Task<bool> UpdateDeliveryStatusAsync(string orderCode, DeliveryStatus deliveryStatus, OrderStatus? orderStatus);
    Task<Order> UpdateOrderAsync(Order order);
}