using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CheckVoucherEligibilityRequest
{
    [Required(ErrorMessage = "Danh sách sản phẩm là bắt buộc")]
    public List<ProductRequest> Items { get; set; } = new List<ProductRequest>();
}
