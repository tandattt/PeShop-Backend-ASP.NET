namespace PeShop.Interfaces
{
    public interface IEmailUtil
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        // string CleanEmailAddress(string email);
    }
}
