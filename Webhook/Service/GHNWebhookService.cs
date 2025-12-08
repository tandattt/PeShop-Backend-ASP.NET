using PeShop.Data.Repositories.Interfaces;
using PeShop.Webhook.Dtos;
using PeShop.Helpers;
using PeShop.Models.Enums;

namespace PeShop.Webhook.Service
{
    public class GHNWebhookService : IGHNWebhookService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPayoutRepository _payoutRepository;
        private readonly ILogger<GHNWebhookService> _logger;

        public GHNWebhookService(
            IOrderRepository orderRepository,
            IPayoutRepository payoutRepository,
            ILogger<GHNWebhookService> logger)
        {
            _orderRepository = orderRepository;
            _payoutRepository = payoutRepository;
            _logger = logger;
        }

        public async Task<GHNWebhookResponse> HandleWebhookAsync(GHNWebhookRequest request)
        {
            _logger.LogInformation("Received GHN webhook: OrderCode={OrderCode}, Status={Status}", 
                request.OrderCode, request.Status);

            // Validate request
            var validationResult = ValidateRequest(request);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Route to appropriate handler based on status
            var status = request.Status.ToLowerInvariant();
            return status switch
            {
                "cancel" => await HandleCancelStatusAsync(request),
                "delivered" => await HandleDeliveredStatusAsync(request),
                _ when GHNStatusMapper.IsSpecialStatus(request.Status) => await HandleSpecialStatusAsync(request),
                _ => await HandleNormalStatusAsync(request)
            };
        }

        private GHNWebhookResponse? ValidateRequest(GHNWebhookRequest request)
        {
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

            return null;
        }

        private async Task<GHNWebhookResponse> HandleCancelStatusAsync(GHNWebhookRequest request)
        {
            var order = await _orderRepository.GetOrderByOrderCodeAsync(request.OrderCode);
            if (order == null)
            {
                _logger.LogWarning("Order not found for cancel status: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = $"Order not found: {request.OrderCode}",
                    StatusCode = 404
                };
            }

            // Map status to enums
            var deliveryStatus = GHNStatusMapper.MapToDeliveryStatus(request.Status);
            var orderStatus = GHNStatusMapper.MapToOrderStatus(request.Status);

            if (deliveryStatus == null)
            {
                _logger.LogWarning("Invalid cancel status received: {Status}", request.Status);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = $"Invalid status: {request.Status}",
                    StatusCode = 400
                };
            }

            // Update order status
            var orderUpdated = await _orderRepository.UpdateDeliveryStatusAsync(
                request.OrderCode, 
                deliveryStatus.Value, 
                orderStatus);

            if (!orderUpdated)
            {
                _logger.LogError("Failed to update order status to cancelled: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "Failed to update order status",
                    StatusCode = 500
                };
            }

            // Update payout status to Cancelled
            var payoutUpdated = await _payoutRepository.UpdatePayoutStatusAsync(order.Id, PayoutStatus.Cancelled);
            
            if (payoutUpdated)
            {
                _logger.LogInformation("Order and payout updated to cancelled: OrderCode={OrderCode}, OrderId={OrderId}", 
                    request.OrderCode, order.Id);
            }
            else
            {
                _logger.LogWarning("Order updated but failed to update payout: OrderCode={OrderCode}", 
                    request.OrderCode);
            }

            return new GHNWebhookResponse
            {
                Success = true,
                Message = "Order and payout cancelled successfully",
                OrderCode = request.OrderCode,
                Status = request.Status,
                DeliveryStatus = deliveryStatus.ToString(),
                OrderStatus = orderStatus?.ToString(),
                Updated = true,
                StatusCode = 200
            };
        }

        private async Task<GHNWebhookResponse> HandleDeliveredStatusAsync(GHNWebhookRequest request)
        {
            // Map status to enums
            var deliveryStatus = GHNStatusMapper.MapToDeliveryStatus(request.Status);
            var orderStatus = GHNStatusMapper.MapToOrderStatus(request.Status);

            if (deliveryStatus == null)
            {
                _logger.LogWarning("Invalid delivered status received: {Status}", request.Status);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = $"Invalid status: {request.Status}",
                    StatusCode = 400
                };
            }

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

            if (!result)
            {
                _logger.LogError("Failed to update order: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "Failed to update order status",
                    StatusCode = 500
                };
            }

            // Update payment status to Paid
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
            // Update payout status to Processing
            var payoutUpdated = await _payoutRepository.UpdatePayoutStatusAsync(order.Id, PayoutStatus.Processing);
            if (payoutUpdated)
            {
                _logger.LogInformation("Payout updated to paid: OrderCode={OrderCode}, OrderId={OrderId}", 
                    request.OrderCode, order.Id);
            }
            else
            {
                _logger.LogWarning("Order updated but failed to update payout: OrderCode={OrderCode}", 
                    request.OrderCode);
            }

            return new GHNWebhookResponse
            {
                Success = true,
                Message = "Order and payment status updated successfully",
                OrderCode = request.OrderCode,
                Status = request.Status,
                DeliveryStatus = deliveryStatus.ToString(),
                OrderStatus = orderStatus?.ToString(),
                Updated = true,
                StatusCode = 200
            };
        }

        private async Task<GHNWebhookResponse> HandleNormalStatusAsync(GHNWebhookRequest request)
        {
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

            if (!result)
            {
                _logger.LogError("Failed to update order: OrderCode={OrderCode}", request.OrderCode);
                return new GHNWebhookResponse
                {
                    Success = false,
                    Message = "Failed to update order status",
                    StatusCode = 500
                };
            }

            _logger.LogInformation("Order updated successfully: OrderCode={OrderCode}, DeliveryStatus={DeliveryStatus}, OrderStatus={OrderStatus}", 
                request.OrderCode, deliveryStatus, orderStatus);

            return new GHNWebhookResponse
            {
                Success = true,
                Message = "Order status updated successfully",
                OrderCode = request.OrderCode,
                Status = request.Status,
                DeliveryStatus = deliveryStatus.ToString(),
                OrderStatus = orderStatus?.ToString(),
                Updated = true,
                StatusCode = 200
            };
        }

        private async Task<GHNWebhookResponse> HandleSpecialStatusAsync(GHNWebhookRequest request)
        {
            _logger.LogInformation("Special status received: {Status} for OrderCode={OrderCode}. Skipping update.", 
                request.Status, request.OrderCode);
            
            // TODO: Handle special statuses (return, delivery_fail, etc.) later
            return await Task.FromResult(new GHNWebhookResponse
            {
                Success = true,
                Message = "Special status received",
                Status = request.Status,
                OrderCode = request.OrderCode,
                Updated = false,
                StatusCode = 200
            });
        }
    }
}

