namespace PeShop.Helpers;

public class RequestCounterHelper
{
    private long _totalRequestCount = 0; // Tổng số request (bao gồm cả bị chặn bởi rate limit)
    private long _processedRequestCount = 0; // Số request thực tế được xử lý (sau rate limit)

    /// Tăng counter tổng lên 1 và trả về giá trị mới (đếm tất cả request, kể cả bị chặn)
    public long Increment()
    {
        return Interlocked.Increment(ref _totalRequestCount);
    }

    /// Tăng counter processed lên 1 và trả về giá trị mới (chỉ đếm request được xử lý sau rate limit)
    public long IncrementProcessed()
    {
        return Interlocked.Increment(ref _processedRequestCount);
    }

    /// Lấy tổng số request hiện tại (bao gồm cả bị chặn)
    public long GetTotalCount()
    {
        return Interlocked.Read(ref _totalRequestCount);
    }

    /// Lấy số request thực tế được xử lý (sau rate limit)
    public long GetProcessedCount()
    {
        return Interlocked.Read(ref _processedRequestCount);
    }

    /// Reset cả 2 counter về 0
    public void Reset()
    {
        Interlocked.Exchange(ref _totalRequestCount, 0);
        Interlocked.Exchange(ref _processedRequestCount, 0);
    }

    /// Reset chỉ counter tổng
    public void ResetTotal()
    {
        Interlocked.Exchange(ref _totalRequestCount, 0);
    }

    /// Reset chỉ counter processed
    public void ResetProcessed()
    {
        Interlocked.Exchange(ref _processedRequestCount, 0);
    }
}

