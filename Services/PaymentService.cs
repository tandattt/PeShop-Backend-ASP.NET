namespace PeShop.Services;
using PeShop.Exceptions;
using PeShop.Services.Interfaces;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Setting;
using PeShop.Models.Enums;
using System.Text.Json;
using PeShop.Data.Repositories.Interfaces;
using System.Diagnostics;
using Hangfire;
using PeShop.Data.Repositories;
using PeShop.Dtos.Responses;
using Microsoft.AspNetCore.Hosting;
public class PaymentService : IPaymentService
{
    private readonly IVnPayUtil _vnPayUtil;
    private readonly IRedisUtil _redisUtil;
    private readonly AppSetting _appSetting;
    private readonly IOrderService _orderService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IOrderRepository _orderRepository;
    private readonly IGHNUtil _ghnUtil;
    private readonly IJobService _jobService;
    public PaymentService(IVnPayUtil vnPayUtil, IRedisUtil redisUtil, AppSetting appSetting, IOrderService orderService, ITransactionRepository transactionRepository, IUserRepository userRepository, IWebHostEnvironment webHostEnvironment, IOrderRepository orderRepository, IGHNUtil ghnUtil, IJobService jobService)
    {
        _vnPayUtil = vnPayUtil;
        _redisUtil = redisUtil;
        _appSetting = appSetting;
        _orderService = orderService;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _webHostEnvironment = webHostEnvironment;
        _orderRepository = orderRepository;
        _ghnUtil = ghnUtil;
        _jobService = jobService;
    }
    public async Task<string> CreatePaymentUrlAsync(string orderId, HttpContext context, string userId)
    {
        var isProcessing = await _redisUtil.GetAsync<OrderPaymentProcessing>($"order_payment_processing_{userId}");
        if (isProcessing != null)
        {
            return isProcessing.PaymentLink;
        }
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
        if (order == null)
        {
            throw new BadRequestException("Đơn hàng không tồn tại");
        }
        var orderResponse = await _orderService.SaveOrderAsync(order, userId, PaymentStatus.Processing, PaymentMethod.VNPay);
        if (!orderResponse.Status)
        {

            return orderResponse.Message;
        }

        var readOrdIds = orderResponse.Data != null ? string.Join(",", orderResponse.Data) : string.Empty;
        Debug.WriteLine($"[CreatePaymentUrlAsync] OrderResponse.Data Count: {orderResponse.Data?.Count ?? 0}");
        Debug.WriteLine($"[CreatePaymentUrlAsync] ReadOrdIds: {readOrdIds}");

        var paymentUrl = await _vnPayUtil.CreatePaymentUrlAsync(new PaymentInformationDto
        {
            Amount = order.AmountTotal,
            OrderID = order.OrderId,
            OrderType = "other",
            ReadOrdIds = readOrdIds,
        }, context, userId);

        // Lưu thông tin payment processing với cấu trúc OrderPaymentProcessing
        var orderPaymentProcessing = new OrderPaymentProcessing
        {
            Time = 15 * 60, // 15 phút = 900 giây
            PaymentLink = paymentUrl
        };
        await _redisUtil.SetAsync($"order_payment_processing_{userId}", orderPaymentProcessing, TimeSpan.FromMinutes(15));
        BackgroundJob.Schedule<IJobService>(service => service.UpdatePaymentStatusFailedInOrderAsync(orderId, userId), TimeSpan.FromMinutes(15));
        return paymentUrl;
    }
    public async Task<string> ProcessCallbackAsync(HttpContext context)
    {
        Console.WriteLine("[ProcessCallbackAsync] ===== BẮT ĐẦU XỬ LÝ CALLBACK =====");
        Console.WriteLine($"[ProcessCallbackAsync] QueryString: {context.Request.QueryString}");
        Console.WriteLine($"[ProcessCallbackAsync] Query Parameters Count: {context.Request.Query.Count}");

        // foreach (var queryParam in context.Request.Query)
        // {
        //     Console.WriteLine($"[ProcessCallbackAsync] Query Param - {queryParam.Key}: {queryParam.Value}");
        // }

        Console.WriteLine("[ProcessCallbackAsync] Đang gọi _vnPayUtil.ProcessCallbackAsync...");
        var response = await _vnPayUtil.ProcessCallbackAsync(context.Request.Query);
        Console.WriteLine($"[ProcessCallbackAsync] Response từ VnPay: Success={response.Success}");
        Console.WriteLine($"[ProcessCallbackAsync] Response.OrderDescription: {response.OrderDescription}");
        var orderId = response.OrderDescription.Split("_")[0];
        var userId = response.OrderDescription.Split("_")[1];
        var recipientName = response.OrderDescription.Split("_")[3];
        var readOrdIds = response.OrderDescription.Split("_")[2];

        Console.WriteLine($"[ProcessCallbackAsync] Parsed - userId: {userId}, orderId: {orderId}, readOrdIds: {readOrdIds}");
        if (response.Success)
        {
            Console.WriteLine("[ProcessCallbackAsync] VnPay callback thành công, bắt đầu xử lý...");
            Console.WriteLine("response.OrderDescription: " + response.OrderDescription);

            Console.WriteLine("[ProcessCallbackAsync] Đang parse OrderDescription...");


            // var order = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
            try
            {
                Console.WriteLine("[ProcessCallbackAsync] Bắt đầu transaction...");
                var result = await _transactionRepository.ExecuteInTransactionAsync(async () =>
                {
                    Console.WriteLine($"[ProcessCallbackAsync] Trong transaction, đang xử lý {readOrdIds.Split(",").Length} order(s)...");

                    var orderIds = readOrdIds.Split(",");
                    for (int i = 0; i < orderIds.Length; i++)
                    {
                        var readOrdId = orderIds[i];
                        Console.WriteLine($"[ProcessCallbackAsync] Xử lý order {i + 1}/{orderIds.Length}: OrderId={readOrdId}");

                        var orderResponse = await _orderService.UpdatePaymentStatusInOrderAsync(readOrdId, userId, PaymentStatus.Paid);
                        Console.WriteLine($"[ProcessCallbackAsync] Order {readOrdId} - Status: {orderResponse.Status}, Message: {orderResponse.Message}");

                        if (!orderResponse.Status)
                        {
                            Console.WriteLine($"[ProcessCallbackAsync] ERROR: Không thể cập nhật trạng thái thanh toán cho order {readOrdId}");
                            throw new Exception("Lỗi khi cập nhật trạng thái thanh toán");
                        }

                        Console.WriteLine($"[ProcessCallbackAsync] Order {readOrdId} đã được cập nhật thành công");
                    }
                    
                    var user = await _userRepository.GetByIdAsync(userId);

                    var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Template", "PaymentSuccess.html");
                    Console.WriteLine($"[PaymentService] ContentRootPath: {_webHostEnvironment.ContentRootPath}");
                    Console.WriteLine($"[PaymentService] TemplatePath: {templatePath}");
                    Console.WriteLine($"[PaymentService] File exists: {File.Exists(templatePath)}");
                    string htmlBody;
                    if (File.Exists(templatePath))
                    {
                        // Lấy thông tin đơn hàng từ Redis để có đầy đủ thông tin
                        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
                        
                        htmlBody = await File.ReadAllTextAsync(templatePath);
                        htmlBody = htmlBody.Replace("{CustomerName}", recipientName);
                        htmlBody = htmlBody.Replace("{RecipientName}", order?.RecipientName ?? recipientName);
                        htmlBody = htmlBody.Replace("{RecipientPhone}", order?.RecipientPhone ?? "");
                        htmlBody = htmlBody.Replace("{ShippingAddress}", order?.UserFullNewAddress ?? "");
                        htmlBody = htmlBody.Replace("{OrderDate}", DateTime.UtcNow.ToString("dd/MM/yyyy"));
                        htmlBody = htmlBody.Replace("{PaymentMethod}", "VNPay");
                        htmlBody = htmlBody.Replace("{TotalAmount}", ((order?.FeeShippingTotal ?? 0) + (order?.OrderTotal ?? response.Amount) - (order?.DiscountTotal ?? 0)).ToString("N0") + " VNĐ");
                        htmlBody = htmlBody.Replace("{SubTotal}", (order?.OrderTotal ?? response.Amount).ToString("N0") + " VNĐ");
                        htmlBody = htmlBody.Replace("{ShippingFee}", (order?.FeeShippingTotal ?? 0).ToString("N0") + " VNĐ");
                        htmlBody = htmlBody.Replace("{Discount}", (order?.DiscountTotal ?? 0).ToString("N0") + " VNĐ");
                        htmlBody = htmlBody.Replace("{Items}", ""); // Tạm thời để trống vì cần thông tin sản phẩm chi tiết
                        htmlBody = htmlBody.Replace("{OrderTrackingUrl}", _appSetting.BaseUrlFrontend + "/orders/" + orderId);
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            BackgroundJob.Enqueue<IEmailUtil>(service => service.SendEmailAsync(user.Email, "Đơn hàng đã được tạo thành công", htmlBody, true));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[PaymentService] Template file not found at: {templatePath}");
                    }
                    // Xóa đơn hàng sau khi thanh toán thành công
                    BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, true));
                    var successUrl = _appSetting.BaseUrlFrontend + "/Payment/success?orderId=" + orderId;
                    Console.WriteLine($"[ProcessCallbackAsync] Transaction thành công, redirect URL: {successUrl}");
                    return successUrl;
                });

                Console.WriteLine("[ProcessCallbackAsync] Transaction hoàn thành thành công");
                Console.WriteLine($"[ProcessCallbackAsync] ===== KẾT THÚC XỬ LÝ CALLBACK THÀNH CÔNG =====");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessCallbackAsync] ===== EXCEPTION TRONG TRANSACTION =====");
                Console.WriteLine($"[ProcessCallbackAsync] Exception Message: {ex.Message}");
                Console.WriteLine($"[ProcessCallbackAsync] Exception StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProcessCallbackAsync] Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("Exception: " + ex.Message);
                
                // Xử lý lỗi: cập nhật trạng thái thanh toán thất bại, xóa Redis, và hủy đơn hàng
                try
                {
                    Console.WriteLine($"[ProcessCallbackAsync] Bắt đầu xử lý rollback cho {readOrdIds.Split(",").Length} order(s)...");
                    var orderIds = readOrdIds.Split(",");
                    
                    foreach (var readOrdId in orderIds)
                    {
                        try
                        {
                            Console.WriteLine($"[ProcessCallbackAsync] Xử lý rollback cho order: {readOrdId}");
                            
                            // 1. Cập nhật payment status sang Failed
                            var updatePaymentResult = await _orderService.UpdatePaymentStatusInOrderAsync(readOrdId, userId, PaymentStatus.Failed);
                            Console.WriteLine($"[ProcessCallbackAsync] Order {readOrdId} - Update payment status: {updatePaymentResult.Status}");
                            
                            // 2. Lấy order để kiểm tra OrderCode (chỉ cần OrderCode, không cần full entity)
                            var order = await _orderRepository.GetOrderByIdAsync(readOrdId, userId);
                            string? orderCode = null;
                            if (order != null)
                            {
                                orderCode = order.OrderCode;
                                
                                // 3. Nếu có OrderCode, gọi GHN API để cancel order trước
                                if (!string.IsNullOrEmpty(orderCode))
                                {
                                    try
                                    {
                                        Console.WriteLine($"[ProcessCallbackAsync] Gọi GHN API để cancel order: {orderCode}");
                                        var cancelResult = await _ghnUtil.CancelOrderAsync(orderCode);
                                        if (cancelResult?.data?.FirstOrDefault()?.result == false)
                                        {
                                            Console.WriteLine($"[ProcessCallbackAsync] Warning: Không thể cancel order trên GHN: {orderCode}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"[ProcessCallbackAsync] Đã cancel order trên GHN thành công: {orderCode}");
                                        }
                                    }
                                    catch (Exception ghnEx)
                                    {
                                        Console.WriteLine($"[ProcessCallbackAsync] Lỗi khi gọi GHN cancel order: {ghnEx.Message}");
                                        // Tiếp tục xử lý dù GHN có lỗi
                                    }
                                }
                            }
                            
                            // 4. Cập nhật order status sang Cancelled và delivery status sang Cancel
                            // Sử dụng method riêng để tránh object cycle
                            var updateStatusResult = await _orderRepository.UpdateOrderStatusAsync(
                                readOrdId, 
                                OrderStatus.Cancelled, 
                                DeliveryStatus.Cancel, 
                                PaymentStatus.Failed, 
                                userId);
                            
                            if (updateStatusResult)
                            {
                                Console.WriteLine($"[ProcessCallbackAsync] Order {readOrdId} đã được cập nhật status sang Cancelled");
                            }
                            else
                            {
                                Console.WriteLine($"[ProcessCallbackAsync] Warning: Không thể cập nhật order status cho {readOrdId}");
                            }
                        }
                        catch (Exception orderEx)
                        {
                            Console.WriteLine($"[ProcessCallbackAsync] Lỗi khi xử lý rollback cho order {readOrdId}: {orderEx.Message}");
                            // Tiếp tục xử lý các order khác
                        }
                    }
                    
                    // 6. Xóa Redis
                    Console.WriteLine($"[ProcessCallbackAsync] Xóa Redis cho orderId: {orderId}, userId: {userId}");
                    BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, true));
                    Console.WriteLine($"[ProcessCallbackAsync] Đã xóa Redis thành công");
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"[ProcessCallbackAsync] Lỗi khi xử lý rollback: {rollbackEx.Message}");
                    Console.WriteLine($"[ProcessCallbackAsync] Rollback StackTrace: {rollbackEx.StackTrace}");
                }
                
                return _appSetting.BaseUrlFrontend + "/Payment/failed?orderId=" + orderId;
            }
        }
        else
        {
            var orderResponse = await _orderService.UpdatePaymentStatusInOrderAsync(orderId, userId, PaymentStatus.Failed);
            // Xóa đơn hàng sau khi thanh toán thất bại
            BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, true));
            Debug.WriteLine("[ProcessCallbackAsync] ===== VNPAY CALLBACK THẤT BẠI =====");
            Debug.WriteLine($"[ProcessCallbackAsync] Response không thành công, throwing exception...");
            return _appSetting.BaseUrlFrontend + "/Payment/failed?orderId=" + orderId;
        }
    }
}