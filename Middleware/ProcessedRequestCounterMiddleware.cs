using PeShop.Helpers;

namespace PeShop.Middleware;


/// Middleware để đếm số lượng request thực tế được xử lý bởi service
/// Middleware này chạy SAU rate limiter, nên chỉ đếm các request đã vượt qua rate limit

public class ProcessedRequestCounterMiddleware
{
    private readonly RequestCounterHelper _requestCounterHelper;
    private readonly RequestDelegate _next;

    public ProcessedRequestCounterMiddleware(
        RequestDelegate next,
        RequestCounterHelper requestCounterHelper)
    {
        _next = next;
        _requestCounterHelper = requestCounterHelper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tiếp tục pipeline trước (rate limiter đã chạy ở đây)
        await _next(context);

        // Sau khi request được xử lý (đã vượt qua rate limit), tăng counter processed
        var currentProcessedCount = _requestCounterHelper.IncrementProcessed();
    }
}

