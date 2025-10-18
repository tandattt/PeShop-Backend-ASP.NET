using PeShop.Dtos.Shared;

namespace PeShop.Dtos.Responses;

public class VoucherEligibilityItem
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
    public VoucherDto Voucher { get; set; } = new VoucherDto();
}

public class VoucherTypeGroup
{
    public string VoucherType { get; set; } = string.Empty; // "System" or "Shop"
    public List<VoucherEligibilityItem> Vouchers { get; set; } = new List<VoucherEligibilityItem>();
    public string? BestVoucherId { get; set; }
}

public class CheckVoucherEligibilityResponse
{
    public List<VoucherTypeGroup> VoucherTypes { get; set; } = new List<VoucherTypeGroup>();
}
