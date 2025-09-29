using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Setting;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IEmailUtil _emailService;
        private readonly AppSetting _appSetting;
        private readonly IMailService _mailService;

        public MailController(IEmailUtil emailService, AppSetting appSetting, IMailService mailService)
        {
            _emailService = emailService;   
            _appSetting = appSetting;
            _mailService = mailService;
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> VerifyMail([FromBody] MailRequest request)
        {
            var result = await _mailService.Verify(request);
            return Ok(new { Message = "Email verified successfully!", Otp = result });
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendMail([FromBody] MailServiceRequest request)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {

                return Unauthorized("Missing Authorization header");
            }

            var token = authHeader.ToString().Replace("Bearer ", "");
            if (token != _appSetting.ApiKeySystem)
            {
                return Forbid("Invalid API key");
            }
            await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
            return Ok(new { Message = "Email sent successfully!" });
        }
    }
}