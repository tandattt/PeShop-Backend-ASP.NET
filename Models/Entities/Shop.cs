using System;
using System.Collections.Generic;
using PeShop.Models.Enums;
namespace PeShop.Models.Entities;

public partial class Shop
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Description { get; set; }

    public uint? FollowersCount { get; set; }

    public uint? FollowingCount { get; set; }

    public string? FullNewAddress { get; set; }

    public string? FullOldAddress { get; set; }

    public string? LogoUrl { get; set; }

    public string? Name { get; set; }

    public string? NewProviceId { get; set; }

    public string? NewWardId { get; set; }

    public string? OldDistrictId { get; set; }

    public string? OldProviceId { get; set; }

    public string? OldWardId { get; set; }

    public uint? PrdCount { get; set; }

    public string? StreetLine { get; set; }

    public ShopStatus Status { get; set; }

    public string? UserId { get; set; }

    public uint? GHNId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User? User { get; set; }

    public virtual ICollection<VoucherShop> VoucherShops { get; set; } = new List<VoucherShop>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
