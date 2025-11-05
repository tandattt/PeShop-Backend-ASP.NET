namespace PeShop.Models.Entities;

public partial class PromotionGift
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int? GiftQuantity { get; set; }

    public string? PromotionId { get; set; }

    public string? ProductId { get; set; }
    public bool? IsDeleted { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual Product? Product { get; set; }
}
