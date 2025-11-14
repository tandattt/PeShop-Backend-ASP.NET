using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PeShop.Constants;
namespace PeShop.Middleware;
public static class RateLimitMiddleware
{
    public static IServiceCollection AddRateLimiterPolicy(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(PolicyConstants.IpPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 50,                 // giới hạn 50 request
                        Window = TimeSpan.FromSeconds(1), // trong vòng 1 giây
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
    public static IServiceCollection AddRateLimiterFLashSalesPolicy(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Policy cho flash sales - cho phép đợi trong hàng đợi
            options.AddPolicy(PolicyConstants.FlashSalesPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        // PermitLimit = 20,                 // tối đa 20 request
                        // Window = TimeSpan.FromMinutes(1), // mỗi 1 phút
                        QueueLimit = int.MaxValue,                  // cho phép 10 request đợi trong hàng đợi
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst // xử lý theo thứ tự cũ nhất trước
                    });
            });

            // Policy chung cho các API khác - có thể đợi
            // options.AddPolicy("api-wait-policy", context =>
            // {
            //     var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            //     return RateLimitPartition.GetFixedWindowLimiter(
            //         partitionKey: ip,
            //         factory: _ => new FixedWindowRateLimiterOptions
            //         {
            //             PermitLimit = 50,                 // tối đa 50 request
            //             Window = TimeSpan.FromMinutes(1), // mỗi 1 phút
            //             QueueLimit = 5,                   // cho phép 5 request đợi trong hàng đợi
            //             QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            //         });
            // });
        });

        return services;
    }
}