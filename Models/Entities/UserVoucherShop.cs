using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class UserVoucherShop
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? UserId { get; set; }

    public string? VoucherShopId { get; set; }

    public int? ClaimedCount { get; set; }

    public int? UsedCount { get; set; }

    public virtual User? User { get; set; }

    public virtual VoucherShop? VoucherShop { get; set; }
}