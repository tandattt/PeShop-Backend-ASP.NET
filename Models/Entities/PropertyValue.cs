using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class PropertyValue
{
    public string Id { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? ImgUrl { get; set; }

    public string? Value { get; set; }
    public int Level { get; set; }

    public string? PropertyProductId { get; set; }

    public virtual PropertyProduct? PropertyProduct { get; set; }

    public virtual ICollection<VariantValue> VariantValues { get; set; } = new List<VariantValue>();
}
