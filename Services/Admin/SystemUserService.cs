using PeShop.Services.Admin.Interfaces;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Exceptions;

namespace PeShop.Services.Admin;

public class SystemUserService : ISystemUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionService _permissionService;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;

    public SystemUserService(
        IUserRepository userRepository, 
        IPermissionService permissionService,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _permissionService = permissionService;
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
    }

    public async Task<PaginationResponse<SystemUserResponse>> GetSystemUsersAsync(GetSystemUsersRequest request)
    {
        var (users, totalCount) = await _userRepository.GetSystemUsersAsync(
            request.Page, 
            request.PageSize, 
            request.Keyword);

        var userResponses = new List<SystemUserResponse>();

        foreach (var user in users)
        {
            // Get first non-User/non-Shop role DisplayName
            var systemRole = user.Roles
                .FirstOrDefault(r => r.Name != "User" && r.Name != "Shop");
            var roleDisplayName = systemRole?.DisplayName ?? string.Empty;

            // Get user permissions
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            userResponses.Add(new SystemUserResponse
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                Avatar = user.Avatar,
                Role = roleDisplayName,
                ListPermission = permissions,
                CreatedAt = user.CreatedAt
            });
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<SystemUserResponse>
        {
            Data = userResponses,
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

    public async Task<SystemUserResponse> GetSystemUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        // Check if user is a system user (has role other than User/Shop)
        var hasSystemRole = user.Roles.Any(r => r.Name != "User" && r.Name != "Shop");
        if (!hasSystemRole)
        {
            throw new BadRequestException("User không phải là System User");
        }

        var systemRole = user.Roles.FirstOrDefault(r => r.Name != "User" && r.Name != "Shop");
        var roleDisplayName = systemRole?.DisplayName ?? string.Empty;
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

        return new SystemUserResponse
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Avatar = user.Avatar,
            Role = roleDisplayName,
            ListPermission = permissions,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<SystemUserResponse> UpdateSystemUserAsync(string userId, UpdateSystemUserRequest request, string updatedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        // Check if user is a system user
        var hasSystemRole = user.Roles.Any(r => r.Name != "User" && r.Name != "Shop");
        if (!hasSystemRole)
        {
            throw new BadRequestException("User không phải là System User");
        }

        // Update basic info
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            // Check if username already exists for another user
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new BadRequestException("Username đã tồn tại");
            }
            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check if email already exists for another user
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new BadRequestException("Email đã tồn tại");
            }
            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            user.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            user.Phone = request.Phone;
        }

        if (request.Avatar != null)
        {
            user.Avatar = request.Avatar;
        }

        // if (!string.IsNullOrWhiteSpace(request.Password))
        // {
        //     user.Password = request.Password;
        // }

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        await _userRepository.UpdateAsync(user);

        // Update permissions if provided
        if (request.ListPermission != null)
        {
            await UpdateUserPermissionsAsync(userId, request.ListPermission, updatedBy);
        }

        // Invalidate permission cache
        _permissionService.InvalidateUserCache(userId);

        // Return updated user
        return await GetSystemUserByIdAsync(userId);
    }

    private async Task UpdateUserPermissionsAsync(string userId, List<string> newPermissionNames, string updatedBy)
    {
        // Get user's roles
        var userRoles = await _userRepository.GetUserRoleEntitiesAsync(userId);
        var systemRole = userRoles.FirstOrDefault(r => r.Name != "User" && r.Name != "Shop");
        
        if (systemRole == null)
        {
            throw new BadRequestException("User không có system role");
        }

        // Get current permissions for the role
        var currentPermissions = await _permissionRepository.GetByRoleIdAsync(systemRole.Id);
        var currentPermissionNames = currentPermissions.Select(p => p.Name).ToList();

        // Get all permissions to validate
        var allPermissions = await _permissionRepository.GetAllAsync();
        var allPermissionDict = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        // Permissions to add
        var permissionsToAdd = newPermissionNames
            .Where(p => !currentPermissionNames.Contains(p) && allPermissionDict.ContainsKey(p))
            .ToList();

        // Permissions to remove
        var permissionsToRemove = currentPermissionNames
            .Where(p => !newPermissionNames.Contains(p))
            .ToList();

        // Add new permissions
        foreach (var permName in permissionsToAdd)
        {
            if (allPermissionDict.TryGetValue(permName, out var permId))
            {
                await _permissionService.AssignPermissionToRoleAsync(systemRole.Id, permId, updatedBy);
            }
        }

        // Remove old permissions
        foreach (var permName in permissionsToRemove)
        {
            var perm = currentPermissions.FirstOrDefault(p => p.Name == permName);
            if (perm != null)
            {
                await _permissionService.RemovePermissionFromRoleAsync(systemRole.Id, perm.Id, updatedBy);
            }
        }

        // Invalidate cache
        _permissionService.InvalidateCache(systemRole.Id);
    }

    public async Task<SystemUserResponse> CreateSystemUserAsync(CreateSystemUserRequest request, string createdBy)
    {
        // Check if username already exists
        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            throw new BadRequestException("Username đã tồn tại");
        }

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new BadRequestException("Email đã tồn tại");
        }

        // Get roles by IDs
        var roles = new List<Models.Entities.Role>();
        foreach (var roleId in request.RoleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new BadRequestException($"Role với ID '{roleId}' không tồn tại");
            }

            // Validate role name (cannot be User or Shop)
            if (role.Name == "User" || role.Name == "Shop")
            {
                throw new BadRequestException($"Role '{role.Name}' không được sử dụng cho System User");
            }

            roles.Add(role);
        }

        // Create new user
        var newUser = new Models.Entities.User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            Name = request.Name,
            Phone = request.Phone,
            Avatar = request.Avatar,
            Password = request.Password,
            Status = Models.Enums.UserStatus.Active,
            HasShop = Models.Enums.HasShop.No,
            Roles = roles,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = createdBy
        };

        var user = await _userRepository.CreateAsync(newUser);

        // Invalidate permission cache
        _permissionService.InvalidateUserCache(user.Id);

        // Return created user
        return await GetSystemUserByIdAsync(user.Id);
    }

    public async Task<StatusResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request, string updatedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        // Check if user is a system user
        var hasSystemRole = user.Roles.Any(r => r.Name != "User" && r.Name != "Shop");
        if (!hasSystemRole)
        {
            throw new BadRequestException("User không phải là System User");
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = request.Password;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;
            await _userRepository.UpdateAsync(user);
            
            return new StatusResponse
            {
                Status = true,
                Message = "Đổi mật khẩu thành công"
            };
        }

        return new StatusResponse
        {
            Status = false,
            Message = "Mật khẩu không được để trống"
        };
    }
}
