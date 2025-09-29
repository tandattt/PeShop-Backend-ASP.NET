using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class OrderDetail
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string? Note { get; set; }

    public decimal? OriginalPrice { get; set; }

    public uint? Quantity { get; set; }

    public decimal? ShippingFee { get; set; }

    public string? OrderId { get; set; }

    public string? ProductId { get; set; }

    public int? VariantId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ICollection<OrderVoucher> OrderVouchers { get; set; } = new List<OrderVoucher>();

    public virtual Product? Product { get; set; }

    public virtual Variant? Variant { get; set; }
}
