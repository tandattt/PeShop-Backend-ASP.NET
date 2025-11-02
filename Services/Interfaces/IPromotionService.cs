
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IPromotionService
{
    Task<List<PromotionResponse>> GetPromotionsByProductAsync(string productId);
    Task<List<PromotionInOrderResponse>> CheckPromotionsInOrderAsync(string orderId, string userId);
}