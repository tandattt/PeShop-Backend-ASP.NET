using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Setting;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller xử lý email và OTP - PUBLIC/API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint gửi email, xác thực OTP.</para>
    /// <para><strong>⚠️ Lưu ý:</strong> Một số endpoint yêu cầu API-KEY để xác thực.</para>
    /// </remarks>
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

        /// <summary>
        /// Xác thực mã OTP - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Xác thực mã OTP đã gửi đến email người dùng</li>
        ///   <li>OTP có hiệu lực trong 5 phút</li>
        ///   <li>Dùng để xác thực trước khi đăng ký hoặc đổi mật khẩu</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "email": "user@example.com",
        ///   "otp": "123456"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> OTP hợp lệ</li>
        ///   <li><strong>400 Bad Request:</strong> OTP không hợp lệ hoặc hết hạn</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Email và mã OTP cần xác thực</param>
        /// <returns>Kết quả xác thực OTP</returns>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _mailService.VerifyOtp(request);
            return Ok(result);
        }

        /// <summary>
        /// Gửi mã OTP đến email - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Gửi mã OTP 6 số đến email người dùng</li>
        ///   <li>OTP có hiệu lực trong 5 phút</li>
        ///   <li>Giới hạn: 3 lần gửi/email/10 phút</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "email": "user@example.com"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> OTP đã được gửi thành công</li>
        ///   <li><strong>400 Bad Request:</strong> Email không hợp lệ hoặc vượt quá giới hạn gửi</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Email cần gửi OTP</param>
        /// <returns>Kết quả gửi OTP</returns>
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] MailRequest request)
        {
            var result = await _mailService.SendOtp(request, false);
            return Ok(result);
        }

        /// <summary>
        /// Gửi email tùy chỉnh - API-KEY
        /// </summary>
        /// <remarks>
        /// <para><strong>🔑 Xác thực:</strong> API-KEY (Bearer Token trong header Authorization)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Gửi email với nội dung tùy chỉnh</li>
        ///   <li>Chỉ dành cho hệ thống nội bộ (internal service)</li>
        ///   <li>Yêu cầu API-KEY hợp lệ trong header</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {API_KEY}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "to": "recipient@example.com",
        ///   "subject": "Tiêu đề email",
        ///   "body": "Nội dung email (HTML supported)"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Email đã gửi thành công</li>
        ///   <li><strong>401 Unauthorized:</strong> Thiếu Authorization header</li>
        ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Thông tin email cần gửi</param>
        /// <returns>Kết quả gửi email</returns>
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

        /// <summary>
        /// Gửi lại mã OTP - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Gửi lại mã OTP mới đến email</li>
        ///   <li>Hủy OTP cũ và tạo OTP mới</li>
        ///   <li>Giới hạn: 3 lần gửi lại/email/10 phút</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "email": "user@example.com"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> OTP mới đã được gửi</li>
        ///   <li><strong>400 Bad Request:</strong> Vượt quá giới hạn gửi lại</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Email cần gửi lại OTP</param>
        /// <returns>Kết quả gửi lại OTP</returns>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] MailRequest request)
        {
            var result = await _mailService.SendOtp(request, true);
            return Ok(result);
        }
    }
}