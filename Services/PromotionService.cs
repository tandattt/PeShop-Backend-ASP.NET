namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Interfaces;
using PeShop.Exceptions;
public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IRedisUtil _redisUtil;
    public PromotionService(IPromotionRepository promotionRepository, IRedisUtil redisUtil)
    {
        _promotionRepository = promotionRepository;
        _redisUtil = redisUtil;
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
                    Quantity = r.Product!.Id == productId ? (uint)(r.Promotion!.PromotionRules.FirstOrDefault()?.Quantity ?? 0) - 1 : (uint)(r.Promotion!.PromotionRules.FirstOrDefault()?.Quantity ?? 0),
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
    public async Task<List<PromotionInOrderResponse>> CheckPromotionsInOrderAsync(string orderId, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");
        if (order == null) throw new BadRequestException("Không tìm thấy đơn hàng");
        
        var promotionInOrderResponses = new List<PromotionInOrderResponse>();
        var processedPromotions = new HashSet<string>(); // Để tránh duplicate
        
        // Duyệt qua từng shop trong order
        foreach (var itemShop in order.ItemShops)
        {
            // Lấy danh sách product ID và quantity trong order của shop này
            var orderProductIds = itemShop.Products.Select(p => p.ProductId).ToList();
            var orderProductQuantities = itemShop.Products
                .GroupBy(p => p.ProductId)
                .ToDictionary(g => g.Key, g => (int)g.Sum(p => p.Quantity));
            
            // Duyệt qua từng product trong order để tìm promotion liên quan
            foreach (var productInOrder in itemShop.Products)
            {
                // Lấy tất cả promotion có product này
                var promotions = await _promotionRepository.GetPromotionByProductAsync(productInOrder.ProductId);
                if (promotions == null || !promotions.Any()) continue;
                
                // Lọc chỉ lấy promotion của shop này
                promotions = promotions.Where(p => p?.ShopId == itemShop.ShopId).ToList();
                
                // Duyệt qua từng promotion
                foreach (var promotion in promotions.Where(p => p != null && !processedPromotions.Contains(p!.Id)))
                {
                    if (promotion!.PromotionRules == null || !promotion.PromotionRules.Any()) continue;
                    if (promotion.PromotionGifts == null || !promotion.PromotionGifts.Any()) continue;
                    
                    processedPromotions.Add(promotion.Id);
                    
                    // Kiểm tra xem các product trong order có match với promotion rules không
                    var allRulesSatisfied = true;
                    var missingProducts = new List<ProductInPromotion>(); // Sản phẩm còn thiếu
                    
                    foreach (var rule in promotion.PromotionRules.Where(r => r.ProductId != null))
                    {
                        var requiredProductId = rule.ProductId!;
                        var requiredQuantity = rule.Quantity ?? 0;
                        
                        // Kiểm tra product này có trong order không
                        var orderQuantity = orderProductQuantities.GetValueOrDefault(requiredProductId, 0);
                        
                        if (orderQuantity >= requiredQuantity)
                        {
                            // Đủ số lượng
                            continue;
                        }
                        else
                        {
                            // Chưa đủ số lượng -> tính số lượng còn thiếu
                            allRulesSatisfied = false;
                            var missingQuantity = requiredQuantity - orderQuantity;
                            
                            if (rule.Product != null)
                            {
                                missingProducts.Add(new ProductInPromotion
                                {
                                    Id = rule.Product.Id,
                                    Name = rule.Product.Name ?? string.Empty,
                                    Image = rule.Product.ImgMain ?? string.Empty,
                                    ReviewCount = rule.Product.ReviewCount ?? 0,
                                    ReviewPoint = rule.Product.ReviewPoint ?? 0,
                                    Price = rule.Product.Price ?? 0,
                                    BoughtCount = rule.Product.BoughtCount ?? 0,
                                    AddressShop = rule.Product.Shop?.NewProviceId ?? string.Empty,
                                    Slug = rule.Product.Slug ?? string.Empty,
                                    Quantity = (uint)missingQuantity // Số lượng còn thiếu
                                });
                            }
                        }
                    }
                    
                    var gift = promotion.PromotionGifts.FirstOrDefault(g => g.Product != null);
                    if (gift != null && gift.Product != null)
                    {
                        var giftQuantity = gift.GiftQuantity ?? 0;
                        var promotionResponse = new PromotionInOrderResponse
                        {
                            PromotionId = promotion.Id,
                            PromotionName = promotion.Name ?? string.Empty,
                            ShopId = itemShop.ShopId,
                            Products = new List<ProductInPromotion>(),
                            PromotionGifts = null
                        };
                        
                        if (allRulesSatisfied)
                        {
                            // Đủ điều kiện -> product rỗng (giá = 0) + gift
                            // promotionResponse.Products.Add(new ProductInPromotion
                            // {
                            //     Id = gift.Product.Id,
                            //     Name = gift.Product.Name ?? string.Empty,
                            //     Image = gift.Product.ImgMain ?? string.Empty,
                            //     ReviewCount = gift.Product.ReviewCount ?? 0,
                            //     ReviewPoint = gift.Product.ReviewPoint ?? 0,
                            //     Price = 0, // Product rỗng nên giá = 0
                            //     BoughtCount = gift.Product.BoughtCount ?? 0,
                            //     AddressShop = gift.Product.Shop?.NewProviceId ?? string.Empty,
                            //     Slug = gift.Product.Slug ?? string.Empty,
                            //     Quantity = giftQuantity
                            // });
                            
                            promotionResponse.PromotionGifts = new PromotionGiftDto
                            {
                                Id = gift.Id,
                                GiftQuantity = giftQuantity,
                                Product = new ProductInPromotionGiftDto
                                {
                                    Id = gift.Product.Id,
                                    Name = gift.Product.Name ?? string.Empty,
                                    Image = gift.Product.ImgMain ?? string.Empty,
                                    ReviewCount = gift.Product.ReviewCount ?? 0,
                                    ReviewPoint = gift.Product.ReviewPoint ?? 0,
                                    Price = gift.Product.Price ?? 0,
                                    BoughtCount = gift.Product.BoughtCount ?? 0,
                                    AddressShop = gift.Product.Shop?.NewProviceId ?? string.Empty,
                                    Slug = gift.Product.Slug ?? string.Empty
                                }
                            };
                        }
                        else
                        {
                            // Chưa đủ điều kiện -> hiển thị số lượng còn thiếu + gift
                            promotionResponse.Products = missingProducts;
                            
                            promotionResponse.PromotionGifts = new PromotionGiftDto
                            {
                                Id = gift.Id,
                                GiftQuantity = giftQuantity,
                                Product = new ProductInPromotionGiftDto
                                {
                                    Id = gift.Product.Id,
                                    Name = gift.Product.Name ?? string.Empty,
                                    Image = gift.Product.ImgMain ?? string.Empty,
                                    ReviewCount = gift.Product.ReviewCount ?? 0,
                                    ReviewPoint = gift.Product.ReviewPoint ?? 0,
                                    Price = gift.Product.Price ?? 0,
                                    BoughtCount = gift.Product.BoughtCount ?? 0,
                                    AddressShop = gift.Product.Shop?.NewProviceId ?? string.Empty,
                                    Slug = gift.Product.Slug ?? string.Empty
                                }
                            };
                        }
                        
                        promotionInOrderResponses.Add(promotionResponse);
                    }
                }
            }
        }
        await _redisUtil.SetAsync($"promotion_in_order_{userId}_{orderId}", promotionInOrderResponses);
        return promotionInOrderResponses;
    }
}