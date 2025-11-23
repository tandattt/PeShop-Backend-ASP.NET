using PeShop.Services.Interfaces;
using PeShop.Models.Enums;
using Hangfire;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Dtos.Job;
using PeShop.Setting;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using System.Text.Json;
using Models.Enums;
using PeShop.Dtos.API;
using PeShop.GlobalVariables;
using PeShop.Exceptions;
namespace PeShop.Services;

public class JobService : IJobService
{
    private readonly IVoucherService _voucherService;
    private readonly IRedisUtil _redisUtil;

    private readonly IUserRankRepository _userRankRepository;
    private readonly IRankService _rankService;
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    public JobService(IVoucherService voucherService, IRedisUtil redisUtil, AppSetting appSetting, IApiHelper apiHelper, IOrderRepository orderRepository, IUserRankRepository userRankRepository, IRankService rankService, IProductRepository productRepository, ITransactionRepository transactionRepository, IBackgroundJobClient backgroundJobClient)
    {
        _voucherService = voucherService;
        _redisUtil = redisUtil;
        _userRankRepository = userRankRepository;
        _rankService = rankService;
        _appSetting = appSetting;
        _apiHelper = apiHelper;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
        _backgroundJobClient = backgroundJobClient;
    }
    public async Task UpdatePaymentStatusFailedInOrderAsync(string orderId, string userId)
    {

        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return;
        }
        if (order.StatusPayment == PaymentStatus.Processing)
        {
            order.StatusPayment = PaymentStatus.Failed;
            Console.WriteLine("order: " + JsonSerializer.Serialize(order));
            await _orderRepository.UpdatePaymentStatusInOrderAsync(order);
        }
    }
    public async Task SetExpireVoucherAsync(string voucherId, DateTime startTime, DateTime endTime, string voucherType)
    {
        var now = DateTime.Now;
        var startDelay = startTime - now;
        var endDelay = endTime - now;

        // --- Kích hoạt ---
        if (startDelay > TimeSpan.Zero)
        {
            if (voucherType == VoucherTypeConstant.System)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active, null),
                    startDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active, null),
                    startDelay
                );
            }
        }
        else
        {
            Console.WriteLine("startDelay: " + startDelay);
            if (voucherType == VoucherTypeConstant.System)
            {
                await _voucherService.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Active, null);
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                await _voucherService.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Active, null);
            }
        }

        // --- Hết hạn ---
        if (endDelay > TimeSpan.Zero)
        {
            if (voucherType == VoucherTypeConstant.System)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherSystemAsync(voucherId, VoucherStatus.Expired, endTime),
                    endDelay
                );
            }
            else if (voucherType == VoucherTypeConstant.Shop)
            {
                BackgroundJob.Schedule<IVoucherService>(
                    s => s.UpdateStatusVoucherShopAsync(voucherId, VoucherStatus.Expired, endTime),
                    endDelay
                );
            }
        }
    }
    public async Task DeleteOrderOnRedisAsync(string orderId, string userId, bool isBank)
    {
        await _redisUtil.DeleteAsync($"order_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"voucher_eligibility_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"calculated_order_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"fee_shipping_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"promotion_in_order_{userId}_{orderId}");
        if (isBank)
        {
            await _redisUtil.DeleteAsync($"order_payment_processing_{userId}");
        }
    }
    public Task SetJobAsync(JobDto dto)
    {

        var runTime = new DateTimeOffset(dto.RunTime, TimeSpan.FromHours(7));
        var delay = runTime - DateTimeOffset.Now;
        
        if (!string.IsNullOrEmpty(dto.Id))
        {
            // Tự đặt ID cho job (ví dụ: voucher_123123)
            var hangfireJobId = _backgroundJobClient.Create(
                () => RunJobAsync(dto),
                new Hangfire.States.ScheduledState(delay)
            );
            
        }
        else
        {
            // Để Hangfire tự tạo ID
            BackgroundJob.Schedule<JobService>(
                s => s.RunJobAsync(dto),
                runTime
            );
        }
        
        return Task.CompletedTask;
    }
    
    public Task DeleteJobAsync(string jobId)
    {
        
        if (!string.IsNullOrEmpty(jobId))
        {
            // Xóa job bằng Hangfire job ID
            BackgroundJob.Delete(jobId);
        }
        else
        {
            throw new BadRequestException("Job ID is required");            
        }
        
        return Task.CompletedTask;
    }

    public async Task RunJobAsync(JobDto dto)
    {
        var apiName = dto.ApiName;
        var jsonData = dto.JsonData;
        var apiUrl = $"{_appSetting.BaseApiBackendJava}{apiName}";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_appSetting.ApiKeySystem}"}
        };
        var response = await _apiHelper.PostAsync<dynamic>(apiUrl, jsonData, headers);
        if (response?.data != null)
        {
            Console.WriteLine("Job executed successfully");
        }
        else
        {
            Console.WriteLine("Job executed failed");
        }
    }

    public async Task UpdateUserRankAsync(string userId, decimal totalSpent)
    {
        var userRank = await _userRankRepository.GetUserRankByIdAsync(userId);

        if (userRank == null)
        {
            var rank = await _rankService.GetRankByTotalSpentAsync(totalSpent);
            if (rank == null)
            {
                return;
            }
            await _userRankRepository.CreateUserRankAsync(new UserRank
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RankId = rank.Id,
                TotalSpent = totalSpent,
                AchievedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            });
        }
        else
        {
            userRank.TotalSpent += totalSpent;
            var rank = await _rankService.GetRankByTotalSpentAsync(userRank.TotalSpent ?? totalSpent);
            if (userRank.RankId != rank.Id)
            {
                userRank.AchievedAt = DateTime.UtcNow;
            }
            userRank.RankId = rank.Id;
            userRank.UpdatedAt = DateTime.UtcNow;
            await _userRankRepository.UpdateUserRankAsync(userRank);
        }
    }
    public async Task UpdateReviewInProductAsync(string productId, float rating)
    {
        var product = await _productRepository.GetProductByIdAsync(productId);
        if (product == null)
        {
            return;
        }
        product.ReviewCount++;
        product.ReviewPoint = (product.ReviewPoint * product.ReviewCount + rating) / product.ReviewCount;
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateProductAsync(product);
    }
    public async Task ApproveProductJobAsync()
    {
        var key = KeyConstants.ApproveProduct;
        // var products = await _productRepository.GetProductsByStatusAsync(ProductStatus.Pending);
        var productInRedis = await _redisUtil.GetAsync<ApproveProductDto>(key);
        if (productInRedis == null || productInRedis.Products.Count == 0)
        {
            return;
        }
        HandleProduct.IsRunningHandleProduct = true;
        var productDtos = productInRedis.Products.  Select(p => new ProductApi
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            ImageUrl = p.ImageUrl ?? string.Empty
        }).ToList();
        await _transactionRepository.ExecuteInTransactionAsync(async () =>
        {
            var response = await _apiHelper.PostAsync<ApproveProductResponse>($"{_appSetting.BaseApiFlask}/approve_products", new ApproveProductDto { Products = productDtos });
            // await _apiHelper.PostAsync<dynamic>($"{_appSetting.BaseApiFlask}/add_to_vector", new ApproveProductDto { Products = productDtos });
            // Console.WriteLine("Response: " + JsonSerializer.Serialize(response));
            if (response?.Results != null)
            {
                // Console.WriteLine("Response: " + JsonSerializer.Serialize(response));
                foreach (var result in response.Results)
                {
                    if (result.IsApprove == true)
                    {
                        // Console.WriteLine("Approve product: " + result.Id);
                        await _productRepository.UpdateProductStatusAsync(result.Id, ProductStatus.Active, null);
                    }
                    else if (result.IsApprove == false)
                    {
                        // Console.WriteLine("Lock product: " + result.Id);
                        await _productRepository.UpdateProductStatusAsync(result.Id, ProductStatus.Locked, result.Reason);
                    }
                    else
                    {
                        await _productRepository.UpdateProductStatusAsync(result.Id, ProductStatus.Unspecified, null);
                    }
                }
            }
        });
        HandleProduct.IsRunningHandleProduct = false;
    }
    public async Task ReloadCacheFlaskAsync()
    {
        var response = await _apiHelper.GetAsync<dynamic>($"{_appSetting.BaseApiFlask}/reload_cache");
        Console.WriteLine(response);
    }
}
