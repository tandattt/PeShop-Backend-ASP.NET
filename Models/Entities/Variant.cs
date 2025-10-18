using System;
using System.Collections.Generic;
using PeShop.Models.Enums;
namespace PeShop.Models.Entities;

public partial class Variant
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public decimal? Price { get; set; }

    public uint? Quantity { get; set; }

    public VariantStatus? Status { get; set; }
    public string? ProductId { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<VariantValue> VariantValues { get; set; } = new List<VariantValue>();
}
