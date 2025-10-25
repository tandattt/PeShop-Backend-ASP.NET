using System;

namespace PeShop.Models.Entities
{
    public partial class PlatformFee
    {
        public uint Id { get; set; }

        public string CategoryId { get; set; } = null!;

        public uint Fee { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }

        public virtual Category Category { get; set; } = null!;
    }
}
