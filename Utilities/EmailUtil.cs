using PeShop.Interfaces;
using PeShop.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace PeShop.Utilities
{
    
public class EmailUtil : IEmailUtil
{
    private readonly SmtpSettings _settings;

    public EmailUtil(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        email.Body = new TextPart(isHtml ? "html" : "plain")
        {
            Text = body
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
}
