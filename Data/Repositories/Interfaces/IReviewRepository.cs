using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<bool> HasReviewAsync(string orderId,string productId,string userId);
    Task<bool> CreateReviewAsync(Review review);
}