using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Admin.Interfaces;

public interface ISystemUserService
{
    Task<PaginationResponse<SystemUserResponse>> GetSystemUsersAsync(GetSystemUsersRequest request);
    Task<SystemUserResponse> GetSystemUserByIdAsync(string userId);
    Task<SystemUserResponse> CreateSystemUserAsync(CreateSystemUserRequest request, string createdBy);
    Task<SystemUserResponse> UpdateSystemUserAsync(string userId, UpdateSystemUserRequest request, string updatedBy);
    Task<StatusResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request, string updatedBy);
}
