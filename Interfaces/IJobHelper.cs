namespace PeShop.Interfaces;

public interface IJobHelper
{
    Task SetExpireVoucherSystem(string voucherSystemId, DateTime startTime, DateTime endTime);
    Task SetExpireVoucherShop(string voucherShopId, DateTime startTime, DateTime endTime);
}
