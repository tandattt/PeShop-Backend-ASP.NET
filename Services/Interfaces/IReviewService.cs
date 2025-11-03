using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services.Interfaces;

public interface IReviewService
{
    Task<bool> IsAllowReviewAsync(string orderId,string productId,string userId);
    Task<StatusResponse> CreateReviewAsync(CreateReviewRequest request,string userId);
    Task<ListReviewResponseDto> GetReviewByProductAsync(string productId);
}