using PeShop.Services.Interfaces;
using PeShop.Models.Enums;
using Hangfire;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
namespace PeShop.Services;

public class JobService : IJobService
{
    private readonly IVoucherService _voucherService;
    private readonly IRedisUtil _redisUtil;
    public JobService(IVoucherService voucherService, IRedisUtil redisUtil)
    {
        _voucherService = voucherService;
        _redisUtil = redisUtil;
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
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active),
                    startDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active),
                    startDelay
                );
            }
        }
        else
        {
            Console.WriteLine("startDelay: " + startDelay);
            if (voucherType == VoucherTypeConstant.System)
            {
                await _voucherService.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active);
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                await _voucherService.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active);
            }
        }

        // --- Hết hạn ---
        if (endDelay > TimeSpan.Zero)
        {
            if (voucherType == VoucherTypeConstant.System)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Expired),
                    endDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Expired),
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
        if (isBank)
        {
            await _redisUtil.DeleteAsync($"order_payment_processing_{userId}_{orderId}");
        }
    }
}
