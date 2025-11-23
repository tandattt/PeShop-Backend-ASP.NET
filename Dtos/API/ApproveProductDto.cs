namespace PeShop.Dtos.API;
public class ApproveProductDto
{
    public List<ProductApi> Products{ get; set; } = new List<ProductApi>();
}

public class ProductApi{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class ApproveProductResponse{
    public List<ApproveProductResponseDto> Results { get; set; } = new List<ApproveProductResponseDto>();
}
public class ApproveProductResponseDto{
   public string Id { get; set; } = string.Empty;
    public bool? IsApprove { get; set; } = null;
    public string Reason { get; set; } = string.Empty;
}