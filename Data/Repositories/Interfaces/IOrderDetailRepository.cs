namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IOrderDetailRepository
{
    Task<OrderDetail?> CreateOrderDetailAsync(OrderDetail orderDetail);
}