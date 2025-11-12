using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class Order
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public decimal? DiscountPrice { get; set; }
    public decimal? ShippingFee { get; set; }
    public string? DeliveryAddress { get; set; }

    public DeliveryStatus? DeliveryStatus { get; set; }

    public decimal? FinalPrice { get; set; }

    public decimal? OriginalPrice { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public OrderStatus? StatusOrder { get; set; }

    public PaymentStatus? StatusPayment { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }

    public decimal? SystemVoucherDiscount { get; set; }
    public decimal? ShopVoucherDiscount { get; set; }
    public string? OrderCode { get; set; }

    public string? ShopId { get; set; }

    public string? UserId { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderVoucher> OrderVouchers { get; set; } = new List<OrderVoucher>();

    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();

    public virtual Shop? Shop { get; set; }

    public virtual User? User { get; set; }
}
