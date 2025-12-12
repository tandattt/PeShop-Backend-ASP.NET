using PeShop.Models.Enums;
namespace PeShop.Dtos.Requests;

public class UpdateVoucherStatusJobRequest
{
    public string VoucherId { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; }
}

