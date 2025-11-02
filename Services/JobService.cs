using PeShop.Services.Interfaces;
using PeShop.Models.Enums;
using Hangfire;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Dtos.Job;
using PeShop.Setting;
using PeShop.Data.Repositories.Interfaces;
namespace PeShop.Services;

public class JobService : IJobService
{
    private readonly IVoucherService _voucherService;
    private readonly IRedisUtil _redisUtil;
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    private readonly IOrderRepository _orderRepository;
    public JobService(IVoucherService voucherService, IRedisUtil redisUtil, AppSetting appSetting, IApiHelper apiHelper, IOrderRepository orderRepository)
    {
        _voucherService = voucherService;
        _redisUtil = redisUtil;
        _appSetting = appSetting;
        _apiHelper = apiHelper;
        _orderRepository = orderRepository;
    }
    public async Task UpdatePaymentStatusFailedInOrderAsync(string orderId, string userId)
    {
        
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return;
        }
        if (order.StatusPayment == PaymentStatus.Processing){
            order.StatusPayment = PaymentStatus.Failed;
            await _orderRepository.UpdatePaymentStatusInOrderAsync(order);
        }
    }
    public async Task SetExpireVoucherAsync(string voucherId, DateTime startTime, DateTime endTime, string voucherType)
    {
        var now = DateTime.Now;
        var startDelay = startTime - now;
        var endDelay = endTime - now;

        // --- Kích hoạt ---
        if (startDelay > TimeSpan.Zero)
        {
            if (voucherType == VoucherTypeConstant.System)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active, null),
                    startDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active, null),
                    startDelay
                );
            }
        }
        else
        {
            Console.WriteLine("startDelay: " + startDelay);
            if (voucherType == VoucherTypeConstant.System)
            {
                await _voucherService.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active, null);
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                await _voucherService.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active, null);
            }
        }

        // --- Hết hạn ---
        if (endDelay > TimeSpan.Zero)
        {
            if (voucherType == VoucherTypeConstant.System)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Expired, endTime),
                    endDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Expired, endTime),
                    endDelay
                );
            }
        }
    }
    public async Task DeleteOrderOnRedisAsync(string orderId, string userId, bool isBank )
    {
        await _redisUtil.DeleteAsync($"order_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"voucher_eligibility_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"calculated_order_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"fee_shipping_{userId}_{orderId}");
        
        await _redisUtil.DeleteAsync($"promotion_in_order_{userId}_{orderId}");
        if (isBank)
        {
            await _redisUtil.DeleteAsync($"order_payment_processing_{userId}_{orderId}");
        }
    }
    public async Task SetJobAsync(JobDto dto)
    {
       
       var runTime = new DateTimeOffset(dto.RunTime,TimeSpan.FromHours(7));
       BackgroundJob.Schedule<JobService>(
        s => s.RunJobAsync(dto),
        runTime
       );
    }

    public async Task RunJobAsync(JobDto dto)
    {
        var apiName = dto.ApiName;
        var jsonData = dto.JsonData;
        var apiUrl = $"{_appSetting.BaseApiBackendJava}{apiName}";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_appSetting.ApiKeySystem}"}
        };
        var response = await _apiHelper.PostAsync<dynamic>(apiUrl, jsonData, headers);
        if (response?.data != null)
        {
            Console.WriteLine("Job executed successfully");
        }
        else
        {
            Console.WriteLine("Job executed failed");
        }
    }
}
