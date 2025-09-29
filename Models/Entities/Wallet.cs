using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class Wallet
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public decimal? Balance { get; set; }

    public string? ShopId { get; set; }

    public virtual Shop? Shop { get; set; }
}
