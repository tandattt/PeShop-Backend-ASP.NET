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
using System.Text.Json.Nodes;
using Models.Enums;
using PeShop.Dtos.API;
using PeShop.GlobalVariables;
using PeShop.Exceptions;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
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
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public JobService(IVoucherService voucherService, IRedisUtil redisUtil, AppSetting appSetting, IApiHelper apiHelper, IOrderRepository orderRepository, IUserRankRepository userRankRepository, IRankService rankService, IProductRepository productRepository, ITransactionRepository transactionRepository, IBackgroundJobClient backgroundJobClient, IWebHostEnvironment webHostEnvironment)
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
        _webHostEnvironment = webHostEnvironment;
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
        await _redisUtil.DeleteAsync($"fee_shipping_v2_{userId}_{orderId}");

        await _redisUtil.DeleteAsync($"promotion_in_order_{userId}_{orderId}");
        if (isBank)
        {
            await _redisUtil.DeleteAsync($"order_payment_processing_{userId}");
        }
    }
    public Task SetJobAsync(JobDto dto)
    {
        Console.WriteLine("SetJobAsync: " + JsonSerializer.Serialize(dto));

        // ✅ Xử lý DateTime UTC đúng cách
        DateTime utcDateTime = dto.RunTime.Kind == DateTimeKind.Utc
            ? dto.RunTime
            : DateTime.SpecifyKind(dto.RunTime, DateTimeKind.Utc);

        var runTime = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
        var delay = runTime - DateTimeOffset.UtcNow;
        
        // ✅ Convert JsonElement thành string JSON để lưu vào Hangfire (tránh serialize metadata)
        // Lưu JsonData dưới dạng string để Hangfire có thể deserialize lại đúng
        if (dto.JsonData is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                // Nếu là string, lấy string đó
                dto.JsonData = jsonElement.GetString() ?? string.Empty;
            }
            else
            {
                // Nếu là object, convert thành JSON string
                // Deserialize JsonElement thành object rồi serialize lại thành string
                try
                {
                    var obj = JsonSerializer.Deserialize<object>(jsonElement);
                    dto.JsonData = JsonSerializer.Serialize(obj);
                }
                catch
                {
                    // Fallback: dùng GetRawText() nếu có thể
                    try
                    {
                        dto.JsonData = jsonElement.GetRawText();
                    }
                    catch
                    {
                        // Cuối cùng: giữ nguyên, sẽ xử lý ở RunJobAsync
                        Console.WriteLine("Warning: Could not convert JsonElement to string");
                    }
                }
            }
        }
        
        Console.WriteLine("dto.JsonData after convert: " + JsonSerializer.Serialize(dto.JsonData));
        if (!string.IsNullOrEmpty(dto.Id))
        {
            // Tự đặt ID cho job từ dto.Id
            // Hangfire không hỗ trợ set custom ID trực tiếp cho scheduled job
            // Nên dùng Create với ScheduledState, sau đó có thể dùng dto.Id để track
            var hangfireJobId = _backgroundJobClient.Create(
                () => RunJobAsync(dto),
                new Hangfire.States.ScheduledState(delay)
            );
            // Lưu mapping giữa dto.Id và hangfireJobId vào file
            SaveJobMapping(dto.Id, hangfireJobId);
            Console.WriteLine($"Job created with custom ID: {dto.Id}, Hangfire JobId: {hangfireJobId}");
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
        if (string.IsNullOrEmpty(jobId))
        {
            throw new BadRequestException("Job ID is required");
        }

        // Tìm Hangfire JobId từ custom ID
        var hangfireJobId = GetHangfireJobId(jobId);

        if (!string.IsNullOrEmpty(hangfireJobId))
        {
            // Xóa job bằng Hangfire job ID
            BackgroundJob.Delete(hangfireJobId);
            // Xóa mapping khỏi file
            RemoveJobMapping(jobId);
            Console.WriteLine($"Deleted job with custom ID: {jobId}, Hangfire JobId: {hangfireJobId}");
        }
        else
        {
            // Nếu không tìm thấy mapping, coi như jobId là Hangfire JobId trực tiếp
            BackgroundJob.Delete(jobId);
            Console.WriteLine($"Deleted job with Hangfire JobId: {jobId}");
        }

        return Task.CompletedTask;
    }

    private string JobMappingFilePath
    {
        get
        {
            var dataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "data");
            // Đảm bảo thư mục data tồn tại
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            return Path.Combine(dataPath, "job_mappings.json");
        }
    }

    private void SaveJobMapping(string customId, string hangfireJobId)
    {
        try
        {
            var mappings = LoadJobMappings();
            mappings[customId] = hangfireJobId;

            var json = JsonSerializer.Serialize(mappings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JobMappingFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving job mapping: {ex.Message}");
        }
    }

    private string? GetHangfireJobId(string customId)
    {
        try
        {
            var mappings = LoadJobMappings();
            return mappings.TryGetValue(customId, out var jobId) ? jobId : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting job mapping: {ex.Message}");
            return null;
        }
    }

    private void RemoveJobMapping(string customId)
    {
        try
        {
            var mappings = LoadJobMappings();
            if (mappings.Remove(customId))
            {
                var json = JsonSerializer.Serialize(mappings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JobMappingFilePath, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing job mapping: {ex.Message}");
        }
    }

    private Dictionary<string, string> LoadJobMappings()
    {
        try
        {
            if (File.Exists(JobMappingFilePath))
            {
                var json = File.ReadAllText(JobMappingFilePath);
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return mappings ?? new Dictionary<string, string>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading job mappings: {ex.Message}");
        }
        return new Dictionary<string, string>();
    }

    public async Task RunJobAsync(JobDto dto)
    {
        var apiName = dto.ApiName;
        var apiUrl = $"{apiName}";
        var headers = new Dictionary<string, string>
        {
            { "API-KEY", $"{_appSetting.ApiKeySystem}"}
        };
        Console.WriteLine("API-KEY: " + headers["API-KEY"]);
        
        // ✅ Convert JsonData thành object để gửi (không phải string JSON)
        object? requestBody = null;
        
        if (dto.JsonData is string jsonString)
        {
            // Nếu là string JSON, parse thành object
            try
            {
                requestBody = JsonSerializer.Deserialize<object>(jsonString);
            }
            catch
            {
                // Nếu không parse được, giữ nguyên string
                requestBody = jsonString;
            }
        }
        else if (dto.JsonData is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                // Nếu JsonElement là string, parse string đó thành object
                var str = jsonElement.GetString();
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        requestBody = JsonSerializer.Deserialize<object>(str);
                    }
                    catch
                    {
                        requestBody = str;
                    }
                }
            }
            else
            {
                // Nếu JsonElement là object, deserialize thành object
                try
                {
                    requestBody = JsonSerializer.Deserialize<object>(jsonElement);
                }
                catch
                {
                    try
                    {
                        requestBody = JsonSerializer.Deserialize<object>(jsonElement.GetRawText());
                    }
                    catch
                    {
                        Console.WriteLine("Warning: Could not convert JsonElement to object");
                    }
                }
            }
        }
        else
        {
            requestBody = dto.JsonData;
        }
        
        Console.WriteLine("requestBody: " + JsonSerializer.Serialize(requestBody));
        var response = await _apiHelper.PostAsync<string>(apiUrl, requestBody, headers);
        if (!string.IsNullOrEmpty(response))
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
        var productDtos = productInRedis.Products.Select(p => new ProductApi
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
