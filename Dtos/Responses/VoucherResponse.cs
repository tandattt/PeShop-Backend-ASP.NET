
using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;

public class VoucherResponse
{
    public string Type { get; set; } = string.Empty;
    public List<VoucherDto> Vouchers { get; set; } = new List<VoucherDto>();
}