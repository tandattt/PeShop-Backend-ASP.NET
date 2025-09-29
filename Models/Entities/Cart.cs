using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class Cart
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public string? ProductId { get; set; }

    public string? UserId { get; set; }

    public int? VariantId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }

    public virtual Variant? Variant { get; set; }
}
