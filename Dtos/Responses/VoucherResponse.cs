using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;

public class VoucherResponse
{
   public List<VoucherGroupResponse> VoucherGroups { get; set; } = new List<VoucherGroupResponse>();
}

public class VoucherGroupResponse{
     public string VoucherType { get; set; } = string.Empty;
    public List<VoucherDto> Vouchers { get; set; } = new List<VoucherDto>();
}