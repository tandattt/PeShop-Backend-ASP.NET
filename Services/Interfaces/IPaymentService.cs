    namespace PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
public interface IPaymentService
{
    Task<string> CreatePaymentUrlAsync(string orderId, HttpContext context, string userId);
    Task<string> ProcessCallbackAsync(HttpContext context);
}