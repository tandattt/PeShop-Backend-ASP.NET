using System;

namespace PeShop.Models.Entities;

public partial class SystemLog
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime? CreateAt { get; set; }
}

