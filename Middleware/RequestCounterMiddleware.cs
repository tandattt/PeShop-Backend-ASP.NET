using PeShop.Helpers;

namespace PeShop.Middleware;


/// Middleware để đếm số lượng request đến service
/// Mỗi request sẽ tăng counter lên 1, không quan tâm đến rate limit

public class RequestCounterMiddleware
{
    private readonly RequestCounterHelper _requestCounterHelper;
    private readonly RequestDelegate _next;

    public RequestCounterMiddleware(
        RequestDelegate next,
        RequestCounterHelper requestCounterHelper)
    {
        _next = next;
        _requestCounterHelper = requestCounterHelper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tăng counter lên 1 cho mỗi request
        var currentCount = _requestCounterHelper.Increment();

        // Tiếp tục pipeline
        await _next(context);
    }
}

