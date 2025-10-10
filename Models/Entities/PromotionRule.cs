namespace PeShop.Models.Entities;

public partial class PromotionRule
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int? Quantity { get; set; }

    public string? PromotionId { get; set; }

    public int? VariantId { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();

    public virtual Variant? Variant { get; set; }
}
