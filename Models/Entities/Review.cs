using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class Review
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? ProductId { get; set; }

    public int? VariantId { get; set; }

    public int? Rating { get; set; }

    public string? Content { get; set; }

    public string? ReplyContent { get; set; }

    public string? UrlImg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public string? OrderId { get; set; }
    public string? ShopId { get; set; }

    public virtual User? User { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Variant? Variant { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Shop? Shop {get;set;}
}

