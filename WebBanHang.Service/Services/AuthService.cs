using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DTOs.Auth;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.IServices;
namespace WebBanHang.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Kiểm tra Email đã tồn tại chưa
            var existingUser = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return new AuthResponseDto { Message = "Email đã được sử dụng!" };

            // 2. Mã hóa mật khẩu (Băm)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Tạo User mới
            var newUser = new User
            {
                Email = dto.Email,
                Phone = dto.Phone,
                // Giả định bạn có thuộc tính PasswordHash trong Model User
                PasswordHash = passwordHash,
                //CreatedAt = DateTime.UtcNow
            };

            var newCart = new Cart
            {
                User = newUser
            };

            await _unitOfWork.User.AddAsync(newUser);
            await _unitOfWork.Cart.AddAsync(newCart);
            await _unitOfWork.SaveAsync();

            // 4. Gán quyền mặc định (Role Customer = 2)
            var userRole = new UserRole { UserId = newUser.UserId, RoleId = 2 };
            await _unitOfWork.UserRole.AddAsync(userRole);
            await _unitOfWork.SaveAsync();

            return new AuthResponseDto { Message = "Đăng ký thành công!" };
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email == dto.Email, "UserRoles");
            
            if (user == null )
                return new AuthResponseDto { Message = "Sai email " };
            if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return new AuthResponseDto { Message = "Sai  mật khẩu!" };
            // Tạo JWT Access Token (giống cũ, nhưng nên set sống ngắn lại, vd: 15 phút)
            var accessToken = GenerateAccessToken(user);

            // Tạo Refresh Token (chuỗi ngẫu nhiên)
            var refreshToken = GenerateRefreshToken();

            // Lưu Refresh Token vào Database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Sống 7 ngày
            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Đăng nhập thành công!"
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto dto)
        {
            // Lấy Claims từ Access Token cũ (dù đã hết hạn)
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return new AuthResponseDto { Message = "Access Token không hợp lệ" };

            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email == userEmail);

            // Kiểm tra tính hợp lệ của Refresh Token
            if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResponseDto { Message = "Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." };

            // Cấp lại cặp Token mới
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Cập nhật lại vào DB để xoay vòng (Rotate) Token, tăng tính bảo mật
            user.RefreshToken = newRefreshToken;
            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Đã cấp lại Token thành công!"
            };
        }

        // ================= CÁC HÀM HỖ TRỢ (PRIVATE) =================

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        } 

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            string role = user.UserRoles.FirstOrDefault()?.RoleId.ToString() ?? string.Empty;
            if(role == "1")
            {
                role = "ADMIN";
            }
            else if(role == "2")
            {
                role = "CUSTOMER";
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        }),
                Expires = DateTime.UtcNow.AddMinutes(30), // thời gian Access Token  sống 
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                ValidateLifetime = false // Quan trọng: Bỏ qua check hết hạn vì ta biết nó đã hết hạn
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token không hợp lệ");

            return principal;
        }
    }
}
