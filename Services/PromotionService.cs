namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    public PromotionService(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }
    public async Task<List<PromotionResponse>> GetPromotionsByProductAsync(string productId)
    {
        // var promotions = await _promotionRepository.GetProductInPromotionAsync(productId, shopId);
        var promotions = await _promotionRepository.GetPromotionByProductAsync(productId);
        List<PromotionResponse> promotionResponses = new List<PromotionResponse>();
        foreach (var promotion in promotions)
        {
            if (promotion == null) continue;
            var firstGift = promotion.PromotionGifts?.FirstOrDefault();
            var promotionResponse = new PromotionResponse(){
                PromotionId = promotion.Id,
                PromotionName = promotion.Name ?? string.Empty,
                
                Products = promotion.PromotionRules?.Where(r => r.Product != null).Select(r => new ProductInPromotion
                {
                    Id = r.Product!.Id,
                    Name = r.Product.Name ?? string.Empty,
                    Image = r.Product.ImgMain ?? string.Empty,
                    ReviewCount = r.Product.ReviewCount ?? 0,
                    ReviewPoint = r.Product.ReviewPoint ?? 0,
                    Price = r.Product.Price ?? 0,
                    BoughtCount = r.Product.BoughtCount ?? 0,
                    AddressShop = r.Product.Shop?.NewProviceId ?? string.Empty,
                    Slug = r.Product.Slug ?? string.Empty,
                    Quantity = r.Product!.Id == productId ? (r.Promotion!.PromotionRules.FirstOrDefault()?.Quantity ?? 0) - 1 : (r.Promotion!.PromotionRules.FirstOrDefault()?.Quantity ?? 0),
                }).Where(r => r.Quantity > 0).ToList() ?? new List<ProductInPromotion>(),
                PromotionGifts = firstGift?.Product != null ? new PromotionGiftDto
                {
                    Id = firstGift.Product.Id,
                    GiftQuantity = firstGift.GiftQuantity ?? 0,
                    Product = new ProductInPromotionGiftDto
                    {
                        Id = firstGift.Product.Id,
                        Name = firstGift.Product.Name ?? string.Empty,
                        Image = firstGift.Product.ImgMain ?? string.Empty,
                        ReviewCount = firstGift.Product.ReviewCount ?? 0,
                        ReviewPoint = firstGift.Product.ReviewPoint ?? 0,
                        Price = firstGift.Product.Price ?? 0,
                        BoughtCount = firstGift.Product.BoughtCount ?? 0,
                        AddressShop = firstGift.Product.Shop?.NewProviceId ?? string.Empty,
                        Slug = firstGift.Product.Slug ?? string.Empty,
                    }
                } : null,
            };
            promotionResponses.Add(promotionResponse);
        }
        return promotionResponses;
    }
  
}