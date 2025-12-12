using PeShop.Models.Enums;

namespace PeShop.Dtos.Requests;

public class AGetVoucherRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Code { get; set; } = null; // Filter theo mã voucher
    public VoucherValueType? Type { get; set; } = null; // Filter theo loại (Percentage hoặc FixedAmount)
    public VoucherStatus? Status { get; set; } = null; // Filter theo trạng thái (Active, Inactive, Expired)
    public string? SortOrder { get; set; } = "newest"; // "newest" or "oldest"
    public DateTime? DateFrom { get; set; } = null; // Filter từ ngày
    public DateTime? DateTo { get; set; } = null; // Filter đến ngày
}

