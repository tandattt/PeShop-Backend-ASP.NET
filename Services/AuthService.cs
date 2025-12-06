using PeShop.Data.Repositories;
using PeShop.Dtos.Common;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
using PeShop.Exceptions;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Constants;
using PeShop.Extensions;
using PeShop.Utilities;
using PeShop.Services.Interfaces;

namespace PeShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;
        private readonly IRoleRepository _roleRepository;
        private readonly IRedisUtil _redisUtil;
        private readonly IShopRepository _shopRepository;
        private readonly IPermissionService _permissionService;
        public AuthService(IUserRepository userRepository, IJwtHelper jwtHelper, IRoleRepository roleRepository, IRedisUtil redisUtil, IShopRepository shopRepository, IPermissionService permissionService)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _roleRepository = roleRepository;
            _redisUtil = redisUtil;
            _shopRepository = shopRepository;
            _permissionService = permissionService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var email = EmailUtil.CleanEmailAddress(request.Email);
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.Password != request.Password)
            {
                throw new BadRequestException("Email/Username hoặc mật khẩu không đúng");
            }
            else if (user.Status != UserStatus.Active)
            {
                throw new BadRequestException("Tài khoản đã bị khóa");
            }

            var userRoles = await _userRepository.GetUserRolesAsync(user.Id);
            var userPermissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            var shop = await _shopRepository.GetShopByUserIdAsync(user.Id);
            // Console.WriteLine("shop?.Id: " + shop?.Id);
            // Console.WriteLine("userPermissions count: " + (userPermissions?.Count ?? 0));
            // if (userPermissions != null && userPermissions.Count > 0)
            // {
            //     Console.WriteLine("userPermissions: " + string.Join(", ", userPermissions));
            // }
            var accessPayload = new JwtPayloadDto 
            {
                Sub = user.Id,
                ShopId = shop != null ? shop.Id : "",
                Authorities = userRoles,
                Permissions = userPermissions ?? new List<string>(),
                TokenType = "access",
                TimeLive = 24,
            };

            var refreshPayload = new JwtPayloadDto
            {
                Sub = user.Id,
                ShopId = shop != null ? shop.Id : "",
                TokenType = "refresh",
                TimeLive = 48
            };

            var accessToken = _jwtHelper.GenerateToken(accessPayload);
            var refreshToken = _jwtHelper.GenerateToken(refreshPayload);
            // Console.WriteLine(accessToken);
            // Console.WriteLine(refreshToken);
            // BackgroundJob.Enqueue<IEmailUtil>(service => service.SendEmailAsync(user.Email, "Login Success", "Login Success", false));
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }

        public async Task<StatusResponse> RegisterAsync(RegisterRequest request)
        {
            var email = await _redisUtil.GetAsync($"Email_Verified:{request.Key}");
            if (email == null)
            {
                throw new BadRequestException("Key không đúng");
            }
            await _redisUtil.DeleteAsync($"Email_Verified:{request.Key}");
            

            if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                throw new BadRequestException("Username đã được sử dụng");
            }
            try
            {
                var Role = await _roleRepository.GetByNameAsync(RoleConstants.User);
                if (Role == null)
                {
                    throw new BadRequestException("Role not found");
                }
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
                    Username = request.Username,
                    Name = request.Name ?? null,
                    Password = request.Password,
                    Status = UserStatus.Active,
                    HasShop = HasShop.No,
                    Phone = request.Phone ?? null,
                    Gender = request.Gender ?? null,
                    Roles = new List<Role> { Role },
                    CreatedAt = DateTime.UtcNow
                };


                var user = await _userRepository.CreateAsync(newUser);
                if (user == null)
                {
                    throw new BadRequestException("tạo user thất bại");
                }
                
                return new StatusResponse { Status = true };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Lỗi server ");
            }

        }


        // public async Task<string?> GetUserIdFromTokenAsync(string token)
        // {
        //     return _jwtUtil.GetUserIdFromToken(token);
        // }

        // public async Task<List<string>> GetRolesFromTokenAsync(string token)
        // {
        //     return _jwtUtil.GetRolesFromToken(token);
        // }

        // public async Task<string?> GetShopIdFromTokenAsync(string token)
        // {
        //     return _jwtUtil.GetShopIdFromToken(token);
        // }
    }
}
