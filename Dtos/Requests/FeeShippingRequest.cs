using PeShop.Models.Entities;
using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Requests;

public class FeeShippingRequest
{
    public string ShopId { get; set; }
    // public string ProductId { get; set; }
    public string UserNewFullAddress { get; set; }
    public string UserNewProviceId { get; set; }
    public string UserNewWardId { get; set; }
    public List<ProductRequest> Product { get; set; }
}

public class ListFeeShippingRequest{
    public List<FeeShippingRequest> ListFeeShipping { get; set; }
}

public class ProductRequest{
   [Required(ErrorMessage = "ProductId là bắt buộc")]
    public string ProductId { get; set; } = string.Empty;
    
    public string? VariantId { get; set; } // null thì lấy giá product
    
    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
}