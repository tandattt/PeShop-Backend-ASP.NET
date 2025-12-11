using PeShop.Services.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Enums;
using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Interfaces;
using PeShop.Setting;
using Hangfire;
namespace PeShop.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    private readonly IShopRepository _shopRepository;

    public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository, IProductRepository productRepository, IApiHelper apiHelper, AppSetting appSetting,IShopRepository shopRepository)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _apiHelper = apiHelper;
        _appSetting = appSetting;
        _shopRepository = shopRepository;
    }
    public async Task<bool> IsAllowReviewAsync(string orderId, string productId, string userId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return false;
        }

        // Kiểm tra order status phải là PickedUp (3)
        if (order.StatusOrder != OrderStatus.PickedUp)
        {
            return false;
        }
        // Kiểm tra chưa có review
        var hasReview = await _reviewRepository.HasReviewAsync(order.Id, productId, userId);
        return !hasReview; // Return true nếu chưa có review
    }
    public async Task<Dictionary<(string OrderId, string ProductId), bool>> GetAllowReviewStatusBatchAsync(List<(string OrderId, string ProductId)> items, string userId)
    {
        var result = new Dictionary<(string OrderId, string ProductId), bool>();
        
        if (items == null || !items.Any())
        {
            return result;
        }

        // Lấy tất cả orderIds duy nhất
        var orderIds = items.Select(x => x.OrderId).Distinct().ToList();
        
        // Lấy tất cả orders cùng lúc
        var orders = await _orderRepository.GetOrdersByIdsAsync(orderIds, userId);
        var ordersDict = orders.ToDictionary(o => o.Id, o => o);
        
        // Lấy tất cả existing reviews cùng lúc
        var existingReviews = await _reviewRepository.GetExistingReviewsBatchAsync(items, userId);
        
        // Check từng item
        foreach (var item in items)
        {
            if (!ordersDict.TryGetValue(item.OrderId, out var order))
            {
                result[item] = false;
                continue;
            }

            // Kiểm tra order status phải là PickedUp (3)
            if (order.StatusOrder != OrderStatus.PickedUp)
            {
                result[item] = false;
                continue;
            }

            // Kiểm tra chưa có review
            var hasReview = existingReviews.Contains((item.OrderId, item.ProductId));
            result[item] = !hasReview;
        }

        return result;
    }
    public async Task<StatusResponse> CreateReviewAsync(CreateReviewRequest request, string userId)
    {
        var isAllowReview = await IsAllowReviewAsync(request.OrderId, request.ProductId, userId);
        if (!isAllowReview)
        {
            return new StatusResponse { Status = false, Message = "Bạn không có quyền đánh giá sản phẩm" };
        }
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
        string shopId = await _shopRepository.GetShopIdByProductIdAsync(request.ProductId);
        var review = new Review
        {
            OrderId = request.OrderId,
            ShopId = shopId,
            ProductId = request.ProductId,
            VariantId = int.Parse(request.VariantId),
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
        BackgroundJob.Enqueue<IJobService>(x => x.UpdateReviewInProductAsync(request.ProductId, request.Rating));
        return new StatusResponse { Status = true, Message = "Đánh giá sản phẩm thành công" };
    }
    public async Task<ListReviewResponseDto> GetReviewByProductAsync(string productId)
    {
        var reviews = await _reviewRepository.GetReviewByProductAsync(productId);
        var reviewResponseDtos = reviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id,
            Content = r.Content ?? string.Empty,
            ReplyContent = r.ReplyContent ?? string.Empty,
            Rating = r.Rating ?? 0,
            UrlImg = !string.IsNullOrEmpty(r.UrlImg) ? r.UrlImg.Split(',').ToList() : new List<string>(),
            UserId = r.UserId ?? string.Empty,
            UserName = r.User?.Name ?? string.Empty,
            UserAvatar = r.User?.Avatar ?? string.Empty,
        }).ToList();

        // Get shop information from product if reviews exist, otherwise get from product directly
        string shopId = string.Empty;
        string shopName = string.Empty;
        string shopLogo = string.Empty;

        if (reviews.Any())
        {
            var firstReview = reviews.First();
            if (firstReview.Product != null)
            {
                shopId = firstReview.Product.ShopId ?? string.Empty;
                shopName = firstReview.Product.Shop?.Name ?? string.Empty;
                shopLogo = firstReview.Product.Shop?.LogoUrl ?? string.Empty;
            }
        }

        // If no reviews or shop info not found in reviews, get shop info from product
        if (string.IsNullOrEmpty(shopId))
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product != null)
            {
                shopId = product.ShopId ?? string.Empty;
                shopName = product.Shop?.Name ?? string.Empty;
                shopLogo = product.Shop?.LogoUrl ?? string.Empty;
            }
        }

        return new ListReviewResponseDto
        {
            Reviews = reviewResponseDtos,
            ShopId = shopId,
            ShopName = shopName,
            ShopLogo = shopLogo,
        };
    }
}