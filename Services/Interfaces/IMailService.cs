using PeShop.Dtos.Requests;

namespace PeShop.Services.Interfaces
{
    public interface IMailService
    {
        Task<string> Verify(MailRequest request);
    }
}
