namespace PeShop.Data.Repositories;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly PeShopDbContext _context;
    public OrderDetailRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<OrderDetail?> CreateOrderDetailAsync(OrderDetail orderDetail)
    {
        _context.OrderDetails.Add(orderDetail);
        if (await _context.SaveChangesAsync() > 0) return orderDetail;
        else return null;

    }
}