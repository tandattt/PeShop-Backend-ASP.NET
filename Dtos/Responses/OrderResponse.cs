using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;

public class CreateVirtualOrderResponse : StatusResponse
{
    public OrderVirtualDto? Order { get; set; } = null;
}
