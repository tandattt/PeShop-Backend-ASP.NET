using PeShop.Models.Enums;

namespace PeShop.Dtos.Requests;

public class AUpdateUserStatusRequest
{
    public UserStatus Status { get; set; }
}

