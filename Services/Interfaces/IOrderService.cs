using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Models.Enums;
using PeShop.Dtos.Shared;
namespace PeShop.Services.Interfaces;
public interface IOrderService
{
    Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId);
    Task<CreateVirtualOrderResponse> UpdateVirtualOrder(UpdateVirtualOrderRequest request, string userId);
    Task<CreateVirtualOrderResponse> CalclulateOrderTotal(string orderId, string userId);
    Task<StatusResponse> CreateOrderCODAsync(string orderId, string userId);
    Task<StatusResponse<List<string>>> SaveOrderAsync(OrderVirtualDto orders, string userId, PaymentStatus paymentStatus, PaymentMethod paymentMethod);
    Task<StatusResponse> UpdatePaymentStatusInOrderAsync(string orderId, string userId, PaymentStatus paymentStatus);
    Task<List<OrderResponse>> GetOrderAsync( string userId);
    Task<OrderDetailResponse> GetOrderDetailAsync(string orderId, string userId);
}