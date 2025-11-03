namespace PeShop.Models.Entities;

public partial class PromotionUsage
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }


    public string? OrderId { get; set; }


    public string? PromotionId { get; set; }

    public string? PromotionGiftId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Promotion? Promotion { get; set; }


    public virtual PromotionGift? PromotionGift { get; set; }
}
