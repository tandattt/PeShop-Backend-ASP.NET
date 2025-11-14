using System;

namespace PeShop.Models.Entities;

public partial class UserViewProduct
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual User? User { get; set; }

    public virtual Product? Product { get; set; }
}


