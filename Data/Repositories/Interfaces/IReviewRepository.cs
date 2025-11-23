using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<bool> HasReviewAsync(string orderId,string productId,string userId);
    Task<bool> CreateReviewAsync(Review review);
    Task<List<Review>> GetReviewByProductAsync(string productId);
    Task<HashSet<(string OrderId, string ProductId)>> GetExistingReviewsBatchAsync(List<(string OrderId, string ProductId)> items, string userId);
}