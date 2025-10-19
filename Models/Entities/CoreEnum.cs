using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class CoreEnum
{
    public string Id { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

}
