using PeShop.Services.Interfaces;
using PeShop.Interfaces;
using PeShop.Models.Enums;
using Hangfire;

namespace PeShop.Helpers;

public class JobHelper : IJobHelper
{
    private readonly IVoucherService _voucherService;

    public JobHelper(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    public async Task SetExpireVoucherSystem(string voucherSystemId, DateTime startTime, DateTime endTime)
    {
        var now = DateTime.Now;
        var startDelay = startTime - now;
        var endDelay = endTime - now;

        // --- Kích hoạt ---
        if (startDelay > TimeSpan.Zero)
        {
            BackgroundJob.Schedule<IVoucherService>(
                s => s.UpdateStatusVoucherSystemAsync(voucherSystemId, VoucherStatus.Active),
                startDelay
            );
        }
        else
        {
            Console.WriteLine("startDelay: " + startDelay);
            await _voucherService.UpdateStatusVoucherSystemAsync(voucherSystemId, VoucherStatus.Active);
        }

        // --- Hết hạn ---
        if (endDelay > TimeSpan.Zero)
        {
            BackgroundJob.Schedule<IVoucherService>(
                s => s.UpdateStatusVoucherSystemAsync(voucherSystemId, VoucherStatus.Expired),
                endDelay
            );
        }
    }

    public async Task SetExpireVoucherShop(string voucherShopId, DateTime startTime, DateTime endTime)
    {
        var now = DateTime.Now;
        var startDelay = startTime - now;
        var endDelay = endTime - now;
        Console.WriteLine("now: " + now);
        // --- Kích hoạt ---
        if (startDelay > TimeSpan.Zero)
        {
            BackgroundJob.Schedule<IVoucherService>(
                s => s.UpdateStatusVoucherShopAsync(voucherShopId, VoucherStatus.Active),
                startDelay
            );
        }
        else
        {
            Console.WriteLine("startDelay: " + startDelay);
            await _voucherService.UpdateStatusVoucherShopAsync(voucherShopId, VoucherStatus.Active);
        }

        // --- Hết hạn ---
        if (endDelay > TimeSpan.Zero)
        {
            BackgroundJob.Schedule<IVoucherService>(
                s => s.UpdateStatusVoucherShopAsync(voucherShopId, VoucherStatus.Expired),
                endDelay
            );
        }
    }
}
