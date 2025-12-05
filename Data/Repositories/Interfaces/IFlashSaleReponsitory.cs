using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;
public interface IFlashSaleRepository
{
    Task<List<FlashSaleProduct>> GetFlashSalesAsync(int page, int pageSize, string flashSaleId);
    Task<List<FlashSale>> GetFlashSalesTodayAsync(DateOnly dateTime);
    Task<Dictionary<string, bool>> HasFlashSalesForProductsAsync(List<string> productIds);
    Task<Dictionary<string, uint>> GetFlashSaleDiscountsForProductsAsync(List<string> productIds);
    Task<FlashSaleProduct?> GetActiveFlashSaleProductAsync(string productId);
    Task<Dictionary<string, FlashSaleProduct>> GetFlashSaleProductsByIdsAsync(List<string> flashSaleProductIds);
    Task<Dictionary<string, FlashSaleProduct>> GetActiveFlashSaleProductsAsync(List<string> productIds);
    Task<bool> DecreaseFlashSaleQuantityAsync(string flashSaleProductId, uint quantity);
    Task<int> GetUserFlashSalePurchaseCountAsync(string userId, string flashSaleProductId);
}