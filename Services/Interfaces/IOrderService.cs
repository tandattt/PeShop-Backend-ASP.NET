using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;
public interface IOrderService
{
    Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId);
    Task<CreateVirtualOrderResponse> CalclulateOrderTotal(string orderId, string userId);
}