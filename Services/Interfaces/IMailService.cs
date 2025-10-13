using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Interfaces
{
    public interface IMailService
    {
        Task<StatusResponse> SendOtp(MailRequest request, bool isResend = false);
        Task<VerifyOtpResponse> VerifyOtp(VerifyOtpRequest request);
    }
}
