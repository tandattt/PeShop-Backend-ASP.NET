using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class PropertyProduct
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<PropertyValue> PropertyValues { get; set; } = new List<PropertyValue>();

    public virtual ICollection<VariantValue> VariantValues { get; set; } = new List<VariantValue>();
}
