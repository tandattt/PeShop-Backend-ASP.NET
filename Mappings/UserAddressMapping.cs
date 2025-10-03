using AutoMapper;
using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
namespace PeShop.Mappings;

public class UserAddressMapping : Profile
{
    public UserAddressMapping()
    {
        CreateMap<UserAddress, UserAddressResponse>();
    }
}
