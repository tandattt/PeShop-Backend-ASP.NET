
namespace PeShop.Services.Interfaces;

public interface IJobService
{
    Task SetExpireVoucherAsync(string voucherId, DateTime startTime, DateTime endTime, string voucherType);
    Task DeleteOrderOnRedisAsync(string orderId, string userId, bool isBank);
}
