using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class VoucherShop
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Code { get; set; }

    public decimal? DiscountValue { get; set; }

    public DateTime? EndTime { get; set; }

    public VoucherStatus? Status { get; set; }

    public decimal? MaxdiscountAmount { get; set; }

    public decimal? MinimumOrderValue { get; set; }

    public string? Name { get; set; }

    public uint? Quantity { get; set; }

    public uint? LimitForUser { get; set; }

    public DateTime? StartTime { get; set; }

    public VoucherType? Type { get; set; }

    public string? ShopId { get; set; }

    public virtual Shop? Shop { get; set; }
}
