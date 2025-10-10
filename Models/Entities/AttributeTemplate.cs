using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class AttributeTemplate
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public int? TemplateCategoryId { get; set; }

    public int? TemplateCategoryChildId { get; set; }

    public virtual TemplateCategory? TemplateCategory { get; set; }

    public virtual TemplateCategoryChild? TemplateCategoryChild { get; set; }
}
