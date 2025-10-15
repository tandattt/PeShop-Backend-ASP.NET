namespace PeShop.Interfaces;

public interface IJobHelper
{
    void SetExpireVoucherSystem(string voucherSystemId, DateTime startTime, DateTime endTime);
    void SetExpireVoucherShop(string voucherShopId, DateTime startTime, DateTime endTime);
}
