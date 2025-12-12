namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
public class OrderRepository : IOrderRepository
{
    private readonly PeShopDbContext _context;
    public OrderRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }
    public async Task<OrderVoucher> CreateOrderVoucherAsync(OrderVoucher orderVoucher)
    {
        await _context.OrderVouchers.AddAsync(orderVoucher);
        await _context.SaveChangesAsync();
        return orderVoucher;
    }
    public async Task<Order> GetOrderByIdAsync(string orderId, string userId)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }
    public async Task<List<Order>> GetOrdersByIdsAsync(List<string> orderIds, string userId)
    {
        if (orderIds == null || !orderIds.Any())
        {
            return new List<Order>();
        }
        return await _context.Orders
            .Where(o => orderIds.Contains(o.Id) && o.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<Order?> GetOrderDetailAsync(string orderId, string userId)
    {
        return await _context.Orders
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Variant)
        .ThenInclude(v => v.VariantValues)
        .ThenInclude(vv => vv.PropertyValue)
        .AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }
    public async Task<List<Order>> GetOrderByUserIdAsync(string userId)
    {
        return await _context.Orders
        .Include(o => o.Shop)
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Variant)
        .ThenInclude(v => v.VariantValues)
        .ThenInclude(vv => vv.PropertyValue)
        .Where(o => o.UserId == userId)
        .AsNoTracking()
        .ToListAsync();
    }
    public async Task<bool> UpdatePaymentStatusInOrderAsync(Order order)
    {
        // Chỉ log ID và các field quan trọng để tránh object cycle - hiển thị enum dưới dạng tên
        Console.WriteLine($"Updating order: Id={order.Id}, StatusPayment={order.StatusPayment} ({(int)order.StatusPayment}), StatusOrder={order.StatusOrder} ({(int)order.StatusOrder}), DeliveryStatus={order.DeliveryStatus} ({(int)order.DeliveryStatus})");
        _context.Orders.Update(order);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return true;
        }
        return false;
    }
    
    public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus statusOrder, DeliveryStatus deliveryStatus, PaymentStatus paymentStatus, string updatedBy)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }
        
        order.StatusOrder = statusOrder;
        order.DeliveryStatus = deliveryStatus;
        order.StatusPayment = paymentStatus;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = updatedBy;
        
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    // GHN Webhook methods
    public async Task<Order?> GetOrderByOrderCodeAsync(string orderCode)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
    }

    public async Task<bool> UpdateDeliveryStatusAsync(string orderCode, DeliveryStatus deliveryStatus, OrderStatus? orderStatus)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        
        if (order == null)
        {
            return false;
        }

        order.DeliveryStatus = deliveryStatus;
        
        if (orderStatus.HasValue)
        {
            order.StatusOrder = orderStatus.Value;
        }
        
        order.UpdatedAt = DateTime.UtcNow;
        
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}