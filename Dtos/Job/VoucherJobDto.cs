using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Job;

public class VoucherJobDto
{
    public string? VoucherSystemId { get; set; } = null;
    public string? VoucherShopId { get; set; } = null;
    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    public DateTime StartTime { get; set; }
    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    public DateTime EndTime { get; set; }
}