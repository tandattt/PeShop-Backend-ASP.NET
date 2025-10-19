using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class ImageProduct
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int? SortOrder { get; set; }

    public string? Url { get; set; }

    public string? ProductId { get; set; }

    public virtual Product? Product { get; set; }
}
