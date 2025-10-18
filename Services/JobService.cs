using PeShop.Services.Interfaces;
using PeShop.Models.Enums;
using Hangfire;
using PeShop.Constants;

namespace PeShop.Services;

public class JobService : IJobService
{
    private readonly IVoucherService _voucherService;

    public JobService(IVoucherService voucherService)
    {
        _voucherService = voucherService;
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
}
