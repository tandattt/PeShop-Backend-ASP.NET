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

    public void SetExpireVoucherSystem(string voucherSystemId, DateTime startTime, DateTime endTime)
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
            _voucherService.UpdateStatusVoucherSystemAsync(voucherSystemId, VoucherStatus.Active);
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

    public void SetExpireVoucherShop(string voucherShopId, DateTime startTime, DateTime endTime)
    {
        var now = DateTime.Now;
        var startDelay = startTime - now;
        var endDelay = endTime - now;

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
            _voucherService.UpdateStatusVoucherShopAsync(voucherShopId, VoucherStatus.Active);
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
