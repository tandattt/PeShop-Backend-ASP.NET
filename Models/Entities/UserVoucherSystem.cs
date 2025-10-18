using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class UserVoucherSystem
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? UserId { get; set; }

    public string? VoucherSystemId { get; set; }

    public uint? ClaimedCount { get; set; }

    public uint? UsedCount { get; set; }

    public virtual User? User { get; set; }

    public virtual VoucherSystem? VoucherSystem { get; set; }
}
