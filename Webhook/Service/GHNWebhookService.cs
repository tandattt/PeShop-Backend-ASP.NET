using PeShop.Data.Repositories.Interfaces;
using PeShop.Webhook.Dtos;
using PeShop.Helpers;
using PeShop.Models.Enums;

namespace PeShop.Webhook.Service
{
    public class GHNWebhookService : IGHNWebhookService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GHNWebhookService> _logger;

        public GHNWebhookService(
            IOrderRepository orderRepository,
            ILogger<GHNWebhookService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<GHNWebhookResponse> HandleWebhookAsync(GHNWebhookRequest request)
        {
            _logger.LogInformation("Received GHN webhook: OrderCode={OrderCode}, Status={Status}", 
                request.OrderCode, request.Status);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.OrderCode))
            {
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "OrderCode is required",
                    StatusCode = 400
                };
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "Status is required",
                    StatusCode = 400
                };
            }

            // Check if special status - log only, don't update
            if (GHNStatusMapper.IsSpecialStatus(request.Status))
            {
                _logger.LogInformation("Special status received: {Status} for OrderCode={OrderCode}. Skipping update.", 
                    request.Status, request.OrderCode);
                
                // TODO: Handle special statuses (cancel, return, delivery_fail, etc.) later
                return new GHNWebhookResponse
                {
                    Success = true,
                    Message = "Special status received",
                    Status = request.Status,
                    OrderCode = request.OrderCode,
                    Updated = false,
                    StatusCode = 200
                };
            }

            // Map status to enums
            var deliveryStatus = GHNStatusMapper.MapToDeliveryStatus(request.Status);
            if (deliveryStatus == null)
            {
                _logger.LogWarning("Invalid status received: {Status}", request.Status);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = $"Invalid status: {request.Status}",
                    StatusCode = 400
                };
            }

            var orderStatus = GHNStatusMapper.MapToOrderStatus(request.Status);

            // Check if order exists
            var order = await _orderRepository.GetOrderByOrderCodeAsync(request.OrderCode);
            if (order == null)
            {
                _logger.LogWarning("Order not found: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = $"Order not found: {request.OrderCode}",
                    StatusCode = 404
                };
            }

            // Update order status
            var result = await _orderRepository.UpdateDeliveryStatusAsync(
                request.OrderCode, 
                deliveryStatus.Value, 
                orderStatus);

            if (result)
            {
                // Nếu đã giao hàng (Delivered) thì tự động cập nhật payment status thành Paid
                if (deliveryStatus.Value == DeliveryStatus.Delivered)
                {
                    order.StatusPayment = PaymentStatus.Paid;
                    order.UpdatedAt = DateTime.UtcNow;
                    var paymentUpdateResult = await _orderRepository.UpdatePaymentStatusInOrderAsync(order);
                    
                    if (paymentUpdateResult)
                    {
                        _logger.LogInformation("Order and payment status updated successfully: OrderCode={OrderCode}, DeliveryStatus={DeliveryStatus}, OrderStatus={OrderStatus}, PaymentStatus=Paid", 
                            request.OrderCode, deliveryStatus, orderStatus);
                    }
                    else
                    {
                        _logger.LogWarning("Order status updated but failed to update payment status: OrderCode={OrderCode}", 
                            request.OrderCode);
                    }
                }
                else
                {
                    _logger.LogInformation("Order updated successfully: OrderCode={OrderCode}, DeliveryStatus={DeliveryStatus}, OrderStatus={OrderStatus}", 
                        request.OrderCode, deliveryStatus, orderStatus);
                }
                
                return new GHNWebhookResponse
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    OrderCode = request.OrderCode,
                    DeliveryStatus = deliveryStatus.ToString(),
                    OrderStatus = orderStatus?.ToString(),
                    Updated = true,
                    StatusCode = 200
                };
            }
            else
            {
                _logger.LogError("Failed to update order: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "Failed to update order status",
                    StatusCode = 500
                };
            }
        }
    }
}

