namespace PeShop.Models.Entities;

public partial class PromotionUsage
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int? GiftUsedQuantity { get; set; }

    public int? UsedQuantity { get; set; }

    public DateTime? UsedAt { get; set; }

    public string? OrderDetailId { get; set; }

    public string? PromotionRuleId { get; set; }

    public string? PromotionId { get; set; }

    public virtual OrderDetail? OrderDetail { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual PromotionRule? PromotionRule { get; set; }
}
