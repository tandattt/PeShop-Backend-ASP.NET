using System;
using System.Collections.Generic;
using Models.Enums;

namespace PeShop.Models.Entities;

public partial class Product
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public uint? BoughtCount { get; set; }

    public string? Description { get; set; }

    public uint? Height { get; set; }

    public string? ImgMain { get; set; }

    public uint? Length { get; set; }

    public uint? LikeCount { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public uint? ReviewCount { get; set; }

    public float? ReviewPoint { get; set; }

    public string? Slug { get; set; }

    public uint? ViewCount { get; set; }

    public uint? Weight { get; set; }

    public uint? Width { get; set; }
    public float? score { get; set; }

    public uint? Classify { get; set; }
    public string? Reason { get; set; }

    public string? CategoryChildId { get; set; }

    public string? CategoryId { get; set; }

    public string? ShopId { get; set; }

    public ProductStatus? Status { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual CategoryChild? CategoryChild { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ImageProduct> ImageProducts { get; set; } = new List<ImageProduct>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductInfomation> ProductInfomations { get; set; } = new List<ProductInfomation>();

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<Variant> Variants { get; set; } = new List<Variant>();
}
