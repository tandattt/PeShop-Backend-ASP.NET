using System;

namespace PeShop.Models.Entities;

public partial class UserRank
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RankId { get; set; } = null!;

    public decimal? TotalSpent { get; set; }

    public DateTime? AchievedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual Rank? Rank { get; set; }

    public virtual User? User { get; set; }
}

