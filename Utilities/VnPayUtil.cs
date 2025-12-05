    using Microsoft.Extensions.Configuration;
using PeShop.Interfaces;
using PeShop.Utilities.VnPay;
using PeShop.Dtos.Shared;
using PeShop.Setting;
using System.Text.Json;
namespace PeShop.Utilities
{
    public class VnPayUtil : IVnPayUtil
    {
        private readonly IConfiguration _configuration;
        private readonly VnPaySetting _vnPaySetting;
        public VnPayUtil(IConfiguration configuration, VnPaySetting vnPaySetting)
        {
            _configuration = configuration;
            _vnPaySetting = vnPaySetting;
        }

        public async Task<string> CreatePaymentUrlAsync(PaymentInformationDto model, HttpContext context, string userId)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var random = new Random();
            var randomNumber = random.Next(100000000, 999999999);
            var tick = randomNumber.ToString();
            var pay = new VnPayLibrary();
            var returnUrlPath = _vnPaySetting.PaymentBackReturnUrl;;
            string scheme = context.Request.Scheme;
            string host = context.Request.Host.Value;
            
            var urlCallBack = $"{scheme}://{host}{returnUrlPath}";

            if (model.Amount <= 0)
            {
                throw new ArgumentException("Số tiền không hợp lệ.");
            }

            pay.AddRequestData("vnp_Version", _vnPaySetting.Version);
            pay.AddRequestData("vnp_Command", _vnPaySetting.Command);
            pay.AddRequestData("vnp_TmnCode", _vnPaySetting.TmnCode);
            pay.AddRequestData("vnp_Amount", ((long)(model.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _vnPaySetting.CurrCode);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _vnPaySetting.Locale);
            pay.AddRequestData("vnp_OrderInfo", model.OrderID+"_"+userId+"_"+model.ReadOrdIds+"_"+model.RecipientName);
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = pay.CreateRequestUrl(
                _vnPaySetting.BaseUrl, 
                _vnPaySetting.HashSecret
            );
            
            return await Task.FromResult(paymentUrl);
        }

        public async Task<PaymentResponseDto> ProcessCallbackAsync(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _vnPaySetting.HashSecret);
            Console.WriteLine("response: " + JsonSerializer.Serialize(response));
            return await Task.FromResult(response);
        }

        public PaymentResponseDto GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var pay = new VnPayLibrary();
            return pay.GetFullResponseData(collection, hashSecret);
        }

        public string GetIpAddress(HttpContext context)
        {
            var pay = new VnPayLibrary();
            return pay.GetIpAddress(context);
        }
    }
}
