using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class Payout
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public DateTime? PaidAt { get; set; }

    public decimal? PlatformFee { get; set; }

    public decimal? ShippingFee { get; set; }

    public PayoutStatus? Status { get; set; }

    public string? OrderId { get; set; }

    public string? ShopId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Shop? Shop { get; set; }
}
