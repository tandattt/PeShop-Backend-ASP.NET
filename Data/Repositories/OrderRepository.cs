namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
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
        .ToListAsync();
    }
    public async Task<bool> UpdatePaymentStatusInOrderAsync(Order order)
    {
        Console.WriteLine("order: " + JsonSerializer.Serialize(order));
        _context.Orders.Update(order);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return true;
        }
        return false;
    }
}