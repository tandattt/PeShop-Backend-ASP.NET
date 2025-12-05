using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class TemplateCategory
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public string? CategoryId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<AttributeTemplate> TemplateAttributeTemplates { get; set; } = new List<AttributeTemplate>();

    public virtual Category? Category { get; set; }
}
