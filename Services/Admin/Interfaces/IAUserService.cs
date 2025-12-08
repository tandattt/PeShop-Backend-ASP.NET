using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Services.Admin.Interfaces;

public interface IAUserService
{
    Task<PaginationResponse<AUserResponse>> GetUsersAsync(AGetUserRequest request);
    Task<AUserResponse?> GetUserByIdAsync(string userId);
    Task<StatusResponse> UpdateUserStatusAsync(string userId, AUpdateUserStatusRequest request);
}

