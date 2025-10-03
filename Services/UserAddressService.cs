using PeShop.Services.Interfaces;
using PeShop.Models.Entities;
using PeShop.Dtos.Requests;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Exceptions;
using AutoMapper;
using PeShop.Dtos.Responses;
namespace PeShop.Services;


public class UserAddressService : IUserAddressService
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IMapper _mapper;
    public UserAddressService(IUserAddressRepository userAddressRepository, IMapper mapper)
    {
        _userAddressRepository = userAddressRepository;
        _mapper = mapper;
    }

    public async Task<UserAddressResponse> CreateUserAddressAsync(UserAddressRequest request, string userId)
    {
        UserAddress userAddress = new UserAddress
        {
            Id = Guid.NewGuid().ToString(),
            FullNewAddress = request.FullNewAddress,
            FullOldAddress = request.FullOldAddress,
            Phone = request.Phone,
            NewProviceId = request.NewProviceId,
            NewWardId = request.NewWardId,
            OldDistrictId = request.OldDistrictId,
            OldProviceId = request.OldProviceId,
            OldWardId = request.OldWardId,
            StreetLine = request.StreetLine,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
        };
        
        Console.WriteLine(request.NewWardId);
        var result = await _userAddressRepository.CreateUserAddressAsync(userAddress);
        if (result == null)
        {
            throw new BadRequestException("tạo user address thất bại");
        }
        return _mapper.Map<UserAddressResponse>(result);
    }

    public async Task<UserAddressResponse> UpdateUserAddressAsync(string id, UserAddressRequest request, string userId)
    {
        var existingUserAddress = await _userAddressRepository.GetUserAddressByIdAsync(id);
        if (existingUserAddress == null)
        {
            throw new NotFoundException("UserAddress không tồn tại");
        }

        if (existingUserAddress.UserId != userId)
        {
            throw new ForBidenException("Bạn không có quyền cập nhật địa chỉ này");
        }
        if (request.IsDefault != existingUserAddress.IsDefault)
        {
            var otherUserAddresses = await _userAddressRepository.GetListAddressAsync(userId);
            foreach (var address in otherUserAddresses)
            {
                address.IsDefault = false;
            }
        }
        // Cập nhật thông tin
        existingUserAddress.FullNewAddress = request.FullNewAddress;
        existingUserAddress.FullOldAddress = request.FullOldAddress;
        existingUserAddress.Phone = request.Phone;
        existingUserAddress.NewProviceId = request.NewProviceId;
        existingUserAddress.NewWardId = request.NewWardId;
        existingUserAddress.OldDistrictId = request.OldDistrictId;
        existingUserAddress.OldProviceId = request.OldProviceId;
        existingUserAddress.OldWardId = request.OldWardId;
        existingUserAddress.StreetLine = request.StreetLine;
        existingUserAddress.IsDefault = request.IsDefault;
        existingUserAddress.UpdatedAt = DateTime.UtcNow;

        var result = await _userAddressRepository.UpdateUserAddressAsync(existingUserAddress);
        if (result == null)
        {
            throw new BadRequestException("Cập nhật user address thất bại");
        }
        
        return _mapper.Map<UserAddressResponse>(result);
    }

    public async Task<string> DeleteUserAddressAsync(string id, string userId)
    {
        var existingUserAddress = await _userAddressRepository.GetUserAddressByIdAsync(id);
        if (existingUserAddress == null)
        {
            throw new NotFoundException("UserAddress không tồn tại");
        }

        if (existingUserAddress.UserId != userId)
        {
            throw new ForBidenException("Bạn không có quyền xóa địa chỉ này");
        }

        var result = await _userAddressRepository.DeleteUserAddressAsync(id);
        if (result == null)
        {
            throw new BadRequestException("Xóa user address thất bại");
        }
        return "Xóa user address thành công";
    }

    public async Task<List<UserAddressResponse>> GetListAddressAsync(string userId)
    {
        var result = await _userAddressRepository.GetListAddressAsync(userId);
        return _mapper.Map<List<UserAddressResponse>>(result);
    }

    public async Task<UserAddressResponse> GetAddressDefaultAsync(string userId)
    {
        var result = await _userAddressRepository.GetAddressDefaultAsync(userId);
        return _mapper.Map<UserAddressResponse>(result);
    }
}