using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class Rank
{
    public string Id { get; set; } = null!;

    public decimal MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public RankLevel? RankLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<UserRank> UserRanks { get; set; } = new List<UserRank>();
}

