namespace PeShop.Models.Entities;

public partial class FlashSaleProduct
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? FlashSaleId { get; set; }

    public string? ProductId { get; set; }

    public uint? PercentDecrease { get; set; }

    public uint? Quantity { get; set; }

    public uint? UsedQuantity { get; set; }

    public uint? OrderLimit { get; set; }

    public virtual FlashSale? FlashSale { get; set; }

    public virtual Product? Product { get; set; }
}

