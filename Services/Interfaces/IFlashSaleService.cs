using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;
public interface IFlashSaleService
{
    Task<FlashSaleResponse> GetFlashSalesAsync(int page, int pageSize, string flashSaleId);
    Task<List<FlashSaleTodayResponse>> GetFlashSalesTodayAsync(DateOnly dateTime);
}