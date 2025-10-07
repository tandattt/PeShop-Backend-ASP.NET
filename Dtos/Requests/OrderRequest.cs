namespace Dtos.Requests;

public class OrderRequest
{
    public string UserId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    // public 
}
