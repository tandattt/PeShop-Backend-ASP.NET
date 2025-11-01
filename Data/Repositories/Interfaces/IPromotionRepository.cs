using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IPromotionRepository
{
    Task<List<Product?>> GetProductInPromotionAsync(string productId, string shopId);
    Task<List<Promotion?>> GetPromotionByProductAsync(string productId);
    Task<bool> HasPromotionAsync(string productId);
}