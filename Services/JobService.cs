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
    public JobService(IVoucherService voucherService, IRedisUtil redisUtil, AppSetting appSetting, IApiHelper apiHelper, IOrderRepository orderRepository, IUserRankRepository userRankRepository, IRankService rankService)
    {
        _voucherService = voucherService;
        _redisUtil = redisUtil;
        _userRankRepository = userRankRepository;
        _rankService = rankService;
        _appSetting = appSetting;
        _apiHelper = apiHelper;
        _orderRepository = orderRepository;
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
            await _redisUtil.DeleteAsync($"order_payment_processing_{userId}_{orderId}");
        }
    }
    public async Task SetJobAsync(JobDto dto)
    {

        var runTime = new DateTimeOffset(dto.RunTime, TimeSpan.FromHours(7));
        BackgroundJob.Schedule<JobService>(
         s => s.RunJobAsync(dto),
         runTime
        );
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
}
