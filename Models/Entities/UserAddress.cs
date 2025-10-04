using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class UserAddress
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? FullNewAddress { get; set; }

    public string? FullOldAddress { get; set; }
    public string? Phone { get; set; }

    public string? NewProviceId { get; set; }

    public string? NewWardId { get; set; }

    public string? OldDistrictId { get; set; }

    public string? OldProviceId { get; set; }

    public string? OldWardId { get; set; }

    public string? StreetLine { get; set; }

    public string? UserId { get; set; }

    public bool IsDefault { get; set; } = false;

    public virtual User? User { get; set; }
}
