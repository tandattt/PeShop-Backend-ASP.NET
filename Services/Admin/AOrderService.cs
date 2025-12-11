using PeShop.Services.Admin.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class AOrderService : IAOrderService
{
    private readonly IAOrderRepository _orderRepository;
    
    public AOrderService(IAOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<PaginationResponse<OrderResponse>> GetOrdersAsync(AGetOrderRequest request)
    {
        var orders = await _orderRepository.GetListOrderAsync(request);
        var totalCount = await _orderRepository.GetCountOrderAsync(request);
        
        var orderResponses = orders.Select(o => new OrderResponse
        {
            OrderCode = o.OrderCode ?? string.Empty,
            OrderId = o.Id,
            ShopId = o.ShopId ?? string.Empty,
            ShopName = o.Shop?.Name ?? string.Empty,
            FinalPrice = o.FinalPrice ?? 0,
            PaymentMethod = o.PaymentMethod ?? Models.Enums.PaymentMethod.COD,
            PaymentStatus = o.StatusPayment ?? Models.Enums.PaymentStatus.Unpaid,
            OrderStatus = o.StatusOrder ?? Models.Enums.OrderStatus.Pending,
            HasFlashSale = o.HasFlashSale,
            Items = o.OrderDetails.Select(od => new OrderItemResponse
            {
                ProductId = od.ProductId ?? string.Empty,
                ProductName = od.Product?.Name ?? string.Empty,
                ProductImage = od.Product?.ImgMain ?? string.Empty,
                VariantId = od.VariantId?.ToString() ?? string.Empty,
                VariantValues = new List<PropertyValueForCartDto>(), // Có thể map thêm nếu cần
                Price = od.OriginalPrice ?? 0,
                Quantity = (int)(od.Quantity ?? 0),
                IsAllowReview = false // Admin không cần check review
            }).ToList()
        }).ToList();
        
        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;

        return new PaginationResponse<OrderResponse>
        {
            Data = orderResponses,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPage = hasNextPage ? request.Page + 1 : request.Page,
            PreviousPage = hasPreviousPage ? request.Page - 1 : request.Page
        };
    }
}

