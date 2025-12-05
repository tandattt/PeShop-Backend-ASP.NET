namespace PeShop.Dtos.Responses;

public class PlatformFeeResponse
{
    public uint Id { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public uint Fee { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

