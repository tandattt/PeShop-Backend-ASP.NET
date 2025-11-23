using PeShop.Dtos.Job;

namespace PeShop.Services.Interfaces;

public interface IJobService
{
    Task UpdatePaymentStatusFailedInOrderAsync(string orderId, string userId);
    Task SetExpireVoucherAsync(string voucherId, DateTime startTime, DateTime endTime, string voucherType);
    Task DeleteOrderOnRedisAsync(string orderId, string userId, bool isBank);
    Task SetJobAsync(JobDto dto);
    Task DeleteJobAsync(string jobId);
    Task UpdateUserRankAsync(string userId, decimal totalSpent);

    Task UpdateReviewInProductAsync(string productId, float rating);
    Task ApproveProductJobAsync();

    Task ReloadCacheFlaskAsync();
}
