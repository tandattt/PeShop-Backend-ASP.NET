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
public class PaymentService : IPaymentService
{
    private readonly IVnPayUtil _vnPayUtil;
    private readonly IRedisUtil _redisUtil;
    private readonly AppSetting _appSetting;
    private readonly IOrderService _orderService;
    private readonly ITransactionRepository _transactionRepository;
    public PaymentService(IVnPayUtil vnPayUtil, IRedisUtil redisUtil, AppSetting appSetting, IOrderService orderService, ITransactionRepository transactionRepository)
    {
        _vnPayUtil = vnPayUtil;
        _redisUtil = redisUtil;
        _appSetting = appSetting;
        _orderService = orderService;
        _transactionRepository = transactionRepository;
    }
    public async Task<string> CreatePaymentUrlAsync(string orderId, HttpContext context, string userId)
    {
        var isProcessing = await _redisUtil.GetAsync($"order_payment_processing_{userId}_{orderId}");
        if (isProcessing != null)
        {
            return isProcessing;
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
        await _redisUtil.SetAsync($"order_payment_processing_{userId}_{orderId}", paymentUrl, TimeSpan.FromMinutes(15));
        return paymentUrl;
    }
    public async Task<string> ProcessCallbackAsync(HttpContext context)
    {
        Console.WriteLine("[ProcessCallbackAsync] ===== BẮT ĐẦU XỬ LÝ CALLBACK =====");
        Console.WriteLine($"[ProcessCallbackAsync] QueryString: {context.Request.QueryString}");
        Console.WriteLine($"[ProcessCallbackAsync] Query Parameters Count: {context.Request.Query.Count}");
        
        foreach (var queryParam in context.Request.Query)
        {
            Console.WriteLine($"[ProcessCallbackAsync] Query Param - {queryParam.Key}: {queryParam.Value}");
        }

        Console.WriteLine("[ProcessCallbackAsync] Đang gọi _vnPayUtil.ProcessCallbackAsync...");
        var response = await _vnPayUtil.ProcessCallbackAsync(context.Request.Query);
        Console.WriteLine($"[ProcessCallbackAsync] Response từ VnPay: Success={response.Success}");
        Console.WriteLine($"[ProcessCallbackAsync] Response.OrderDescription: {response.OrderDescription}");
        var orderId = response.OrderDescription.Split("_")[0];
        if (response.Success)
        {
            Console.WriteLine("[ProcessCallbackAsync] VnPay callback thành công, bắt đầu xử lý...");
            Console.WriteLine("response.OrderDescription: " + response.OrderDescription);
            
            Console.WriteLine("[ProcessCallbackAsync] Đang parse OrderDescription...");
            var userId = response.OrderDescription.Split("_")[1];
            
            var readOrdIds = response.OrderDescription.Split("_")[2];
            
            Console.WriteLine($"[ProcessCallbackAsync] Parsed - userId: {userId}, orderId: {orderId}, readOrdIds: {readOrdIds}");
            
            // var order = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
            try 
            { 
                Console.WriteLine("[ProcessCallbackAsync] Bắt đầu transaction...");
                var result = await _transactionRepository.ExecuteInTransactionAsync(async () => {
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
                    // Xóa đơn hàng sau khi thanh toán thành công
                    BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, true));
                    
                    var successUrl = _appSetting.BaseUrlFrontend + "/Payment/success?orderId=" + orderId;
                    Console.WriteLine($"[ProcessCallbackAsync] Transaction thành công, redirect URL: {successUrl}");
                    return await Task.FromResult(successUrl);
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
                throw new Exception("Lỗi khi xử lý thanh toán");
            }
        }
        else
        {
            Debug.WriteLine("[ProcessCallbackAsync] ===== VNPAY CALLBACK THẤT BẠI =====");
            Debug.WriteLine($"[ProcessCallbackAsync] Response không thành công, throwing exception...");
            return _appSetting.BaseUrlFrontend + "/Payment/failed?orderId=" + orderId;
        }
    }
}