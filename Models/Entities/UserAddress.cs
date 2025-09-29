using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class UserAddress
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? FullNewAddress { get; set; }

    public string? FullOldAddress { get; set; }

    public uint? NewProviceId { get; set; }

    public uint? NewWardId { get; set; }

    public uint? OldDistrictId { get; set; }

    public uint? OldProviceId { get; set; }

    public uint? OldWardId { get; set; }

    public string? StreetLine { get; set; }

    public string? UserId { get; set; }

    public virtual User? User { get; set; }
}
