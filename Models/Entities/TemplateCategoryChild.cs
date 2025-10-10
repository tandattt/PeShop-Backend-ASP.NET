using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class TemplateCategoryChild
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public string? CategoryChildId { get; set; }

    public virtual ICollection<AttributeTemplate> AttributeTemplates { get; set; } = new List<AttributeTemplate>();

    public virtual CategoryChild? CategoryChild { get; set; }
}
