using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class TemplateRegisterCategory
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public string? CategoryId { get; set; }

    public virtual ICollection<AttributeTemplate> AttributeTemplates { get; set; } = new List<AttributeTemplate>();

    public virtual Category? Category { get; set; }
}
