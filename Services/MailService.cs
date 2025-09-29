using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using PeShop.Interfaces;
namespace PeShop.Services
{
    public class MailService : IMailService
    {
        private readonly IRedisUtil _redisUtil;
        private readonly IEmailUtil _emailUtil;
        public MailService(IRedisUtil redisUtil, IEmailUtil emailUtil)
        {
            _redisUtil = redisUtil;
            _emailUtil = emailUtil;
        }
        public async Task<string> Verify(MailRequest request)
        {
            var random = new Random();

            string Otp = random.Next(0, 999999).ToString();
            try
            {
                var resultRedis = await _redisUtil.SetAsync($"Email:{request.Gmail}", Otp, TimeSpan.FromMinutes(5));
                if (!resultRedis)
                {
                    throw new Exception("Lỗi khi lưu OTP vào Redis");
                }
                await _emailUtil.SendEmailAsync(request.Gmail, "Xác thực email", $"Mã OTP của bạn là: {Otp}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Otp;
        }
    }
}