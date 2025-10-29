namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
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
    public async Task<bool> UpdatePaymentStatusInOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return true;
    }
}