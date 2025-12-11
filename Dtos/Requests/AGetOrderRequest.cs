namespace PeShop.Dtos.Requests;

public class AGetOrderRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrderCode { get; set; } = null; // Filter theo OrderCode
    public string? SortOrder { get; set; } = "newest"; // "newest" or "oldest"
    public DateTime? DateFrom { get; set; } = null; // Filter từ ngày
    public DateTime? DateTo { get; set; } = null; // Filter đến ngày
}

