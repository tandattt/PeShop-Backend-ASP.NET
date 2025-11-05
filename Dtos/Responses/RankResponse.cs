namespace PeShop.Dtos.Responses;

public class RankResponse
{
    public string Id { get; set; } = string.Empty;
    public string RankLevel { get; set; } = string.Empty; 
    public decimal MinPrice { get; set; } = 0;
    public decimal MaxPrice { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class UserRankResponse
{
    public string Id { get; set; } = string.Empty;
    public string RankId { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; } = 0;
    public DateTime? AchievedAt { get; set; } = null;
    public DateTime? CreatedAt { get; set; } = null;
    public DateTime? UpdatedAt { get; set; } = null;
    public string? CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; } = string.Empty;
}