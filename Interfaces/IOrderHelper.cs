using PeShop.Dtos.Requests;
using PeShop.Dtos.Shared;
namespace PeShop.Interfaces;

public interface IOrderHelper
{
    Task<decimal> CalculateOrderTotalAsync(List<OrderRequest> items);
    Task<List<ItemShop>> GroupItemsByShopAsync(List<OrderRequest> items);
}