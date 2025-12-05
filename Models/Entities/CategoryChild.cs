using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class CategoryChild
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }

    public string? CategoryId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<TemplateCategoryChild> TemplateCategoryChildren { get; set; } = new List<TemplateCategoryChild>();
}
