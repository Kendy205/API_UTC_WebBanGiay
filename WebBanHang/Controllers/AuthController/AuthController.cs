using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Auth;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.AuthController
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (result.Message != "Đăng ký thành công!")
                return BadRequest(ApiResponse<AuthResponseDto>.Failed(result.Message));

            return Ok(ApiResponse<AuthResponseDto>.Succeeded(result, "Đăng ký tài khoản thành công!"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (string.IsNullOrEmpty(result.AccessToken))
                return Unauthorized(ApiResponse<AuthResponseDto>.Failed(result.Message, 401));

            return Ok(ApiResponse<AuthResponseDto>.Succeeded(result, "Đăng nhập thành công!"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            if (string.IsNullOrEmpty(result.AccessToken))
                return Unauthorized(ApiResponse<AuthResponseDto>.Failed(result.Message, 401));

            return Ok(ApiResponse<AuthResponseDto>.Succeeded(result));
        }
    }
}
