using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class Message
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? ShopId { get; set; }

    public string Content { get; set; } = null!;
    public bool Seen { get; set; } = false;

    public SenderType SenderType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual Shop? Shop { get; set; }
}

