using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class OrderDetail
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public decimal? OriginalPrice { get; set; }

    public uint? Quantity { get; set; }

    public string? OrderId { get; set; }

    public string? ProductId { get; set; }

    public int? VariantId { get; set; }

    public string? FlashSaleProductId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Variant? Variant { get; set; }

    public virtual FlashSaleProduct? FlashSaleProduct { get; set; }
}
