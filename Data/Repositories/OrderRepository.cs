namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;

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
}