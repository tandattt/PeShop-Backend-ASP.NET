using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Interfaces;
using PeShop.Setting;
namespace PeShop.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository, IApiHelper apiHelper, AppSetting appSetting)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _apiHelper = apiHelper;
        _appSetting = appSetting;
    }
    public async Task<bool> IsAllowReviewAsync(string orderId, string productId, string userId)
    {
        // Check if user already reviewed this product in this order
        // If already reviewed -> return false (not allow to review)
        // If not reviewed -> return true (allow to review)



        // Check if order is delivered
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return false;
        }
        if (order.StatusOrder != OrderStatus.PickedUp)
        {
            return false;
        }
        var hasReview = await _reviewRepository.HasReviewAsync(order.Id, productId, userId);
        if (!hasReview)
        {
            return true;
        }
        return false;
    }
    public async Task<StatusResponse> CreateReviewAsync(CreateReviewRequest request,string userId)
    {
        var isAllowReview = await IsAllowReviewAsync(request.OrderId, request.ProductId, userId);
        List<string> images = new List<string>();
        foreach (var image in request.Images)
        {
            try
            {
                var uploadUrl = _appSetting.BaseApiBackendJava + "/storage/upload";
                // Thêm subPath như query parameter
                if (!string.IsNullOrEmpty(_appSetting.FolderImagesReview))
                {
                    uploadUrl += "?subPath=" + _appSetting.FolderImagesReview;
                }
                
                Console.WriteLine($"Uploading image to: {uploadUrl}");
                Console.WriteLine($"Image name: {image.FileName}, Size: {image.Length} bytes");
                
                var imageUrl = await _apiHelper.PostFileAsync<string>(
                    uploadUrl, 
                    image, 
                    null, // Không cần form data, chỉ cần file
                    new Dictionary<string, string>
                    {
                        { "API-KEY", _appSetting.ApiKeySystem }
                    });
                
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    images.Add(imageUrl);
                    Console.WriteLine($"Upload successful. Image URL: {imageUrl}");
                }
                else
                {
                    Console.WriteLine($"Upload returned empty URL for image: {image.FileName}");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image {image.FileName}: {ex.Message}");
                // Continue with next image instead of failing entire review
            }
        }
        if (!isAllowReview)
        {
            return new StatusResponse { Status = false, Message = "Bạn không có quyền đánh giá sản phẩm" };
        }
        var review = new Review
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            UserId = userId,
            Content = request.Content,
            Rating = request.Rating,
            UrlImg = images.Any() ? string.Join(",", images) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var result = await _reviewRepository.CreateReviewAsync(review);
        if (!result)
        {
            return new StatusResponse { Status = false, Message = "Đánh giá sản phẩm thất bại" };
        }
        return new StatusResponse { Status = true, Message = "Đánh giá sản phẩm thành công" };
    }
}