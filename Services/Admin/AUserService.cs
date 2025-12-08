using PeShop.Services.Admin.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories;
using PeShop.Exceptions;
using PeShop.Models.Enums;

namespace PeShop.Services.Admin;

public class AUserService : IAUserService
{
    private readonly IUserRepository _userRepository;

    public AUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PaginationResponse<AUserResponse>> GetUsersAsync(AGetUserRequest request)
    {
        var (users, totalCount) = await _userRepository.GetUsersAsync(request.Page, request.PageSize, request.Search);

        var userDtos = users.Select(u => new AUserResponse
        {
            Id = u.Id,
            Username = u.Username ?? string.Empty,
            Email = u.Email ?? string.Empty,
            Name = u.Name ?? string.Empty,
            Phone = u.Phone ?? string.Empty,
            Avatar = u.Avatar ?? string.Empty,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            Status = u.Status,
            Roles = u.Roles.Where(r => r.Name != null).Select(r => r.Name!).ToList()
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<AUserResponse>
        {
            Data = userDtos,
            TotalCount = totalCount,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1,
            NextPage = request.Page < totalPages ? request.Page + 1 : request.Page,
            PreviousPage = request.Page > 1 ? request.Page - 1 : request.Page
        };
    }

    public async Task<AUserResponse?> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        var roles = await _userRepository.GetUserRolesAsync(userId);

        return new AUserResponse
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Avatar = user.Avatar ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Status = user.Status,
            Roles = roles
        };
    }

    public async Task<StatusResponse> UpdateUserStatusAsync(string userId, AUpdateUserStatusRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        var result = await _userRepository.UpdateStatusAsync(userId, request.Status);

        if (!result)
        {
            throw new BadRequestException("Không thể cập nhật trạng thái user");
        }

        return new StatusResponse
        {
            Status = true,
            Message = request.Status == UserStatus.Active ? "Đã kích hoạt tài khoản" : "Đã khóa tài khoản"
        };
    }
}

