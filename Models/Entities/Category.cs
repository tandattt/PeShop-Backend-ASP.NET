using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class Category
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<CategoryChild> CategoryChildren { get; set; } = new List<CategoryChild>();

    public virtual ICollection<TemplateRegisterCategory> TemplateRegisterCategories { get; set; } = new List<TemplateRegisterCategory>();
}
