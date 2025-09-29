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

    public int? TemplateRegisterCategoryId { get; set; }

    public int? TemplateRegisterCategoryChildId { get; set; }

    public virtual TemplateRegisterCategory? TemplateRegisterCategory { get; set; }

    public virtual TemplateRegisterCategoryChild? TemplateRegisterCategoryChild { get; set; }
}
