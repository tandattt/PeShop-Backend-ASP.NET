using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class VoucherSystem
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Code { get; set; }

    public uint? DiscountValue { get; set; }

    public DateTime? EndTime { get; set; }

    public VoucherStatus? Status { get; set; }

    public uint? MaxdiscountAmount { get; set; }

    public uint? MiniumOrderValue { get; set; }

    public string? Name { get; set; }

    public uint? Quantity { get; set; }
    public uint? QuantityUsed { get; set; }
    public uint? LimitForUser { get; set; }

    public DateTime? StartTime { get; set; }

    public VoucherValueType Type { get; set; }
    public virtual ICollection<UserVoucherSystem> UserVoucherSystems { get; set; } = new List<UserVoucherSystem>();
}
