using Models.Enums;

namespace PeShop.Dtos.Requests;

public class ApproveProductRequest
{
    public string ProductId { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } // Chỉ được là Active hoặc Inactive
}

