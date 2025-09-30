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

namespace PeShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;
        private readonly IRedisUtil _redisUtil;
        private readonly IRoleRepository _roleRepository;
        public AuthService(IUserRepository userRepository, IJwtHelper jwtHelper, IRedisUtil redisUtil, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _redisUtil = redisUtil;
            _roleRepository = roleRepository;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || user.Password != request.Password)
            {
                throw new BadRequestException("Email/Username hoặc mật khẩu không đúng");
            }
            else if (user.Status != UserStatus.Active)
            {
                throw new BadRequestException("Tài khoản đã bị khóa");
            }

            var userRoles = await _userRepository.GetUserRolesAsync(user.Id);

            var accessPayload = new JwtPayloadDto
            {
                Sub = user.Id,
                Authorities = userRoles,
                TokenType = "access",
                TimeLive = 24
            };

            var refreshPayload = new JwtPayloadDto
            {
                Sub = user.Id,
                TokenType = "refresh",
                TimeLive = 48
            };

            var accessToken = _jwtHelper.GenerateToken(accessPayload);
            var refreshToken = _jwtHelper.GenerateToken(refreshPayload);
            Console.WriteLine(accessToken);
            Console.WriteLine(refreshToken);
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }

        public async Task<string?> RegisterAsync(RegisterRequest request)
        {

            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                throw new BadRequestException("Email đã được sử dụng");
            }

            if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                throw new BadRequestException("Username đã được sử dụng");
            }

            var otp = await _redisUtil.GetAsync($"Email:{request.Email}")
            ?? throw new BadRequestException("OTP đã hết hạn hoặc chưa được gữi");

            if (otp != request.Otp)
            {
                throw new BadRequestException("OTP không đúng");
            }

            await _redisUtil.DeleteAsync($"Email:{request.Email}");

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
                    Email = request.Email,
                    Username = request.Username,
                    Name = request.Name,
                    Password = request.Password,
                    Status = UserStatus.Active,
                    HasShop = HasShop.No,
                    Phone = request.Phone,
                    Roles = new List<Role> { Role },
                    CreatedAt = DateTime.UtcNow
                };
                

                await _userRepository.CreateAsync(newUser);

                return "tạo tài khoản thành công";
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
