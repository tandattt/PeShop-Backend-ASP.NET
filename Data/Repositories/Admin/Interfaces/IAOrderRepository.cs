using PeShop.Models.Entities;
using PeShop.Dtos.Requests;

namespace PeShop.Data.Repositories.Admin.Interfaces;

public interface IAOrderRepository
{
    Task<int> GetCountOrderAsync(AGetOrderRequest request);
    Task<List<Order>> GetListOrderAsync(AGetOrderRequest request);
}

