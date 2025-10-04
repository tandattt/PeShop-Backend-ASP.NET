using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class VariantValue
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? PropertyProductId { get; set; }

    public int? PropertyValueId { get; set; }

    public int? VariantId { get; set; }

    public virtual PropertyProduct? PropertyProduct { get; set; }

    public virtual PropertyValue? PropertyValue { get; set; }

    public virtual Variant? Variant { get; set; }
}
