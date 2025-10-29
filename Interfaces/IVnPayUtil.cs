using PeShop.Dtos.Shared;

namespace PeShop.Interfaces
{
    public interface IVnPayUtil
    {
        Task<string> CreatePaymentUrlAsync(PaymentInformationDto model, HttpContext context, string userId);
        Task<PaymentResponseDto> ProcessCallbackAsync(IQueryCollection collections);
        PaymentResponseDto GetFullResponseData(IQueryCollection collection, string hashSecret);
        string GetIpAddress(HttpContext context);
    }
}
