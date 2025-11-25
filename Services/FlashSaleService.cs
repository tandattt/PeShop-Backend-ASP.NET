namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
public class FlashSaleService : IFlashSaleService
{
    private readonly IFlashSaleRepository _flashSaleRepository;
    public FlashSaleService(IFlashSaleRepository flashSaleRepository)
    {
        _flashSaleRepository = flashSaleRepository;
    }
    public async Task<FlashSaleResponse> GetFlashSalesAsync(int page, int pageSize, string flashSaleId)
    {
        var flashSales = await _flashSaleRepository.GetFlashSalesAsync(page, pageSize, flashSaleId);
        return new FlashSaleResponse
        {
            FlashSaleId = flashSales.Select(f => f.FlashSale.Id).FirstOrDefault() ?? string.Empty,
            StartTime = flashSales.FirstOrDefault()?.FlashSale.StartTime ?? DateTime.MinValue,
            EndTime = flashSales.FirstOrDefault()?.FlashSale.EndTime ?? DateTime.MinValue,
            Products = flashSales.Select(f => new FlashSaleProductDto
            {
                Id = f.Product.Id,
                Name = f.Product.Name,
                Image = f.Product.ImgMain,
                Price = f.Product.Price ?? 0,
                ReviewCount = f.Product.ReviewCount ?? 0,
                ReviewPoint = f.Product.ReviewPoint ?? 0,
                BoughtCount = f.Product.BoughtCount ?? 0,
                AddressShop = f.Product.Shop?.NewProviceId ?? string.Empty,
                Slug = f.Product.Slug ?? string.Empty,
                ShopId = f.Product.Shop?.Id ?? string.Empty,
                ShopName = f.Product.Shop?.Name ?? string.Empty,
                HasPromotion = null,
                PriceDiscount = (uint)((f.Product.Price ?? 0) - (f.Product.Price ?? 0) * (f.PercentDecrease ?? 0) / 100),
                Quantity = f.Quantity ?? 0,
                UsedQuantity = f.UsedQuantity ?? 0,
                PercentDecrease = f.PercentDecrease ?? 0
            }).ToList()
        };
    }
    public async Task<List<FlashSaleTodayResponse>> GetFlashSalesTodayAsync(DateOnly dateTime)
    {
        var flashSales = await _flashSaleRepository.GetFlashSalesTodayAsync(dateTime);
        var result = new List<FlashSaleTodayResponse>();
        foreach (var flashSale in flashSales)
        {
            var statusText = await ParseStatusText(flashSale.StartTime ?? DateTime.MinValue, flashSale.EndTime ?? DateTime.MinValue);
            var status = await ParseStatus(flashSale.StartTime ?? DateTime.MinValue, flashSale.EndTime ?? DateTime.MinValue);
            
            result.Add(new FlashSaleTodayResponse
            {
                FlashSaleId = flashSale.Id,
                StartTime = flashSale.StartTime ?? DateTime.MinValue,
                EndTime = flashSale.EndTime ?? DateTime.MinValue,
                Status = status,
                StatusText = statusText
            });
        }
        return result;
    }
    private async Task<string> ParseStatusText(DateTime startTime, DateTime endTime)
    {
        if (startTime > DateTime.Now)
        {
            return "Chưa bắt đầu";
        }
        else if (endTime < DateTime.Now)
        {
            return "Đã kết thúc";
        }
        else
        {
            return "Đang diễn ra";
        }
    }
    private async Task<int> ParseStatus(DateTime startTime, DateTime endTime)
    {
        if (startTime > DateTime.Now)
        {
            return 0;
        }
        else if (endTime < DateTime.Now)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

}