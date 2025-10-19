namespace PeShop.Dtos.Requests;
public class GetProductRequest
{
    public string? CategoryId { get; set; } = null;
    public string? CategoryChildId { get; set; } = null;
    public decimal MinPrice { get; set; } = 0;
    public decimal? MaxPrice { get; set; } = null;
    public float? ReviewPoint { get; set; } = null;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetProductByShopRequest :PaginationRequest
{
    public string ShopId { get; set; } = string.Empty;
}