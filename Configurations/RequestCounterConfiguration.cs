using PeShop.Middleware;

namespace PeShop.Configurations;


public static class RequestCounterConfiguration
{
    /// <summary>
    /// Đếm tất cả request (bao gồm cả bị chặn bởi rate limit)
    /// Nên đặt TRƯỚC UseRateLimiter()
    /// </summary>
    public static IApplicationBuilder UseRequestCounter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestCounterMiddleware>();
    }

    /// <summary>
    /// Đếm request thực tế được xử lý (sau rate limit)
    /// Nên đặt SAU UseRateLimiter()
    /// </summary>
    public static IApplicationBuilder UseProcessedRequestCounter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ProcessedRequestCounterMiddleware>();
    }
}

