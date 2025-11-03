using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Models.Enums;
namespace PeShop.Dtos.Responses;

public class CreateVirtualOrderResponse : StatusResponse
{
    public OrderVirtualDto? Order { get; set; } = null;
}
public class OrderResponse{
    public string OrderId { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; } = 0;
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderPaymentProcessing? PaymentProcessing{ get; set; } = null;
    public OrderStatus OrderStatus { get; set; }
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();

}

public class OrderItemResponse{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public string VariantId { get; set; } = string.Empty;
    public List<PropertyValueForCartDto> VariantValues { get; set; } = new List<PropertyValueForCartDto>();
    public decimal Price { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    public bool IsAllowReview { get; set; } = false;
}

public class OrderPaymentProcessing{
    public Double Time { get; set; }
    public string PaymentLink { get; set; } = string.Empty;
}

public class OrderDetailResponse: OrderResponse{

    public DateTime CreatedAt { get; set; } 
    public decimal DiscountPrice { get; set; } = 0;
    public decimal ShippingFee { get; set; } = 0;
    public decimal OriginalPrice { get; set; } = 0;
    
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
}