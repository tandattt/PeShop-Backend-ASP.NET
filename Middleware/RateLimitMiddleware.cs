using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PeShop.Constants;
namespace PeShop.Middleware;
public static class RateLimitMiddleware
{
    public static IServiceCollection AddRateLimiterPolicies(this IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        // ------------------ IP Policy --------------------
        options.AddPolicy(PolicyConstants.IpPolicy, context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = 0
                });
        });

        // ------------------ Flash Sales Policy --------------------
        options.AddPolicy(PolicyConstants.FlashSalesPolicy, context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = int.MaxValue,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        });

        // ------------------ Endpoint-based Policy --------------------
        options.AddPolicy(PolicyConstants.IpPolicyEndpoint, context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"{ip}:{path}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

    });

    return services;
}

}