using PeShop.Dtos.Job;

namespace PeShop.Services.Interfaces;

public interface IJobService
{
    Task UpdatePaymentStatusFailedInOrderAsync(string orderId, string userId);
    Task SetExpireVoucherAsync(string voucherId, DateTime startTime, DateTime endTime, string voucherType);
    Task DeleteOrderOnRedisAsync(string orderId, string userId, bool isBank);
    Task SetJobAsync(JobDto dto);
}
