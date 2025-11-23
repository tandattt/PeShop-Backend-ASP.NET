using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using PeShop.Interfaces;
using PeShop.Utilities;
using PeShop.Dtos.Responses;
using PeShop.Exceptions;
using PeShop.Extensions;
using PeShop.Data.Repositories;
using Microsoft.AspNetCore.Hosting;
namespace PeShop.Services
{
    public class MailService : IMailService
    {
        private readonly IRedisUtil _redisUtil;
        private readonly IEmailUtil _emailUtil;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MailService(IRedisUtil redisUtil, IEmailUtil emailUtil, IUserRepository userRepository, IWebHostEnvironment webHostEnvironment)
        {
            _redisUtil = redisUtil;
            _emailUtil = emailUtil;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<StatusResponse> SendOtp(MailRequest request, bool isResend = false)
        {
            if (await _userRepository.ExistsByEmailAsync(EmailUtil.CleanEmailAddress(request.Email)))
            {
                throw new BadRequestException("Email đã được sử dụng");
            }
            if (await _redisUtil.ExistsAsync($"Email:{EmailUtil.CleanEmailAddress(request.Email)}") && !isResend)
            {
                throw new BadRequestException("Otp đã được gửi vui lòng thử lại sau 1 phút");
            }
            var random = new Random();
            var cleanEmail = EmailUtil.CleanEmailAddress(request.Email);
            string Otp = random.Next(0, 999999).ToString("D6");
            try
            {
                var resultRedis = await _redisUtil.SetAsync($"Email:{cleanEmail}", Otp, TimeSpan.FromMinutes(1));
                if (!resultRedis)
                {
                    throw new BadRequestException("Lỗi khi lưu OTP vào Redis");
                }
                
                // Load HTML template
                var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Template", "Otp.html");
                Console.WriteLine($"[MailService] ContentRootPath: {_webHostEnvironment.ContentRootPath}");
                Console.WriteLine($"[MailService] TemplatePath: {templatePath}");
                Console.WriteLine($"[MailService] File exists: {File.Exists(templatePath)}");
                
                string htmlBody;
                if (File.Exists(templatePath))
                {
                    htmlBody = await File.ReadAllTextAsync(templatePath);
                    htmlBody = htmlBody.Replace("{Otp}", Otp);
                }
                else
                {
                    // Fallback to plain text if template not found
                    Console.WriteLine($"[MailService] Template file not found at: {templatePath}");
                    htmlBody = $"Mã OTP của bạn là: {Otp}";
                }
                
                await _emailUtil.SendEmailAsync(cleanEmail, "Mã OTP - PeShop", htmlBody, true);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
            return new StatusResponse { Status = true };
        }
        public async Task<VerifyOtpResponse> VerifyOtp(VerifyOtpRequest request)
        {
            
            var cleanEmail = EmailUtil.CleanEmailAddress(request.Email);
            var Otp = await _redisUtil.GetAsync($"Email:{cleanEmail}");
            if (Otp == null)
            {
                throw new BadRequestException("OTP đã hết hạn hoặc chưa được gữi");
            }
            if (Otp != request.Otp)
            {
                throw new BadRequestException("OTP không đúng");
            }
            await _redisUtil.DeleteAsync($"Email:{cleanEmail}");
            var key = GenerateRandomKeyExtension.GenerateRandomKey();
            await _redisUtil.SetAsync($"Email_Verified:{key}", cleanEmail, TimeSpan.FromMinutes(1));
            return new VerifyOtpResponse { Status = true, Key = key };
        }

    }
}