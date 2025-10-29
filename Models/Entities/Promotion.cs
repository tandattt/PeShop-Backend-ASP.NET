using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class Promotion
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Name { get; set; }

    public DateTime? StartTime { get; set; }

    public PromotionStatus Status { get; set; }

    public int? TotalUsageLimit { get; set; }

    public string? ShopId { get; set; }

    public virtual ICollection<PromotionGift> PromotionGifts { get; set; } = new List<PromotionGift>();

    public virtual ICollection<PromotionRule> PromotionRules { get; set; } = new List<PromotionRule>();

    public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();

    public virtual Shop? Shop { get; set; }
}
