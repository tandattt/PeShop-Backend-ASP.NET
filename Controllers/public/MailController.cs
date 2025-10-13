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

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _mailService.VerifyOtp(request);
            return Ok(result);
        }
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] MailRequest request)
        {
            var result = await _mailService.SendOtp(request, false);
            return Ok(result);
        }
        [HttpPost("send-mail")]
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
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] MailRequest request)
        {
            var result = await _mailService.SendOtp(request, true);
            return Ok(result);
        }
    }
}