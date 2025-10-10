using System;
using System.Collections.Generic;
using Models.Enums;
using PeShop.Models.Enums;


namespace PeShop.Models.Entities;

public partial class User
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public GenderUser? Gender { get; set; }

    public HasShop? HasShop { get; set; }
    public UserStatus? Status { get; set; }
    public string? Avatar { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<VoucherShop> VoucherShops { get; set; } = new List<VoucherShop>();

    public virtual ICollection<VoucherSystem> VoucherSystems { get; set; } = new List<VoucherSystem>();
}
