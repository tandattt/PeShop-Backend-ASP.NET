using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CheckVoucherEligibilityRequest
{
    [Required(ErrorMessage = "Danh sách sản phẩm là bắt buộc")]
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}

public class OrderItemRequest
{
    [Required(ErrorMessage = "ProductId là bắt buộc")]
    public string ProductId { get; set; } = string.Empty;
    
    public string? VariantId { get; set; } // null thì lấy giá product
    
    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
}
