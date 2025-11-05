namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Interfaces;
using PeShop.Exceptions;
using Models.Enums;
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
            var firstGift = promotion.PromotionGifts?.Where(g => g.IsDeleted != true).FirstOrDefault();
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

                    // Lấy tất cả gifts có product (đã được filter active ở repository)
                    var gifts = promotion.PromotionGifts.Where(g => g.IsDeleted != true && g.Product != null).ToList();
                    if (gifts.Any())
                    {
                        // Map tất cả gifts thành PromotionGiftDto
                        var promotionGiftsList = gifts.Select(g => new PromotionGiftDto
                        {
                            Id = g.Id,
                            GiftQuantity = g.GiftQuantity ?? 0,
                            Product = new ProductInPromotionGiftDto
                            {
                                Id = g.Product!.Id,
                                Name = g.Product.Name ?? string.Empty,
                                Image = g.Product.ImgMain ?? string.Empty,
                                ReviewCount = g.Product.ReviewCount ?? 0,
                                ReviewPoint = g.Product.ReviewPoint ?? 0,
                                Price = g.Product.Price ?? 0,
                                BoughtCount = g.Product.BoughtCount ?? 0,
                                AddressShop = g.Product.Shop?.NewProviceId ?? string.Empty,
                                Slug = g.Product.Slug ?? string.Empty
                            }
                        }).ToList();

                        // Lấy gift đầu tiên để backward compatibility
                        var firstGift = gifts.FirstOrDefault();
                        var promotionResponse = new PromotionInOrderResponse
                        {
                            PromotionId = promotion.Id,
                            PromotionName = promotion.Name ?? string.Empty,
                            ShopId = itemShop.ShopId,
                            Products = new List<ProductInPromotion>(),
                            PromotionGifts = firstGift != null ? promotionGiftsList.FirstOrDefault() : null,
                            PromotionGiftsList = promotionGiftsList
                        };
                        
                        if (!allRulesSatisfied)
                        {
                            // Chưa đủ điều kiện -> hiển thị số lượng còn thiếu
                            promotionResponse.Products = missingProducts;
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