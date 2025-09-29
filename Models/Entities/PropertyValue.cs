using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class PropertyValue
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? ImgUrl { get; set; }

    public string? Value { get; set; }

    public string? PropertyProductId { get; set; }

    public virtual PropertyProduct? PropertyProduct { get; set; }

    public virtual ICollection<VariantValue> VariantValues { get; set; } = new List<VariantValue>();
}
