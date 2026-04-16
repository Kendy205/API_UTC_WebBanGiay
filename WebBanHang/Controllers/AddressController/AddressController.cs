using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.AddressController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController(IAddressService addressService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAddressByUserId()
        {
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null!);
            if (userId == null)
            {
                return BadRequest(ApiResponse<IEnumerable<AddressDto>>.Failed("User ID không hợp lệ."));
            }

            var result = await addressService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<AddressDto>>.Succeeded(result));
        }
        [HttpPost]
        public async Task<IActionResult> CreateAddress(AddressDto addressDto)
        {
            try
            {
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null!);
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AddressDto>.Failed("User ID không hợp lệ."));
                }
                addressDto.UserId = userId;
                await addressService.AddAsync(addressDto);
                return Ok(ApiResponse<AddressDto>.Succeeded(null!, "Thêm địa chỉ thành công!", 201));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Failed($"Lỗi server: {ex.Message}"));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            try
            {
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null!);
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AddressDto>.Failed("User ID không hợp lệ."));
                }
                await addressService.DeleteAsync(id, userId);
                return Ok(ApiResponse<AddressDto>.Succeeded(null!, "Xóa địa chỉ thành công!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Failed($"Lỗi server: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(long id, AddressDto addressDto)
        {
            try
            {
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null!);
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AddressDto>.Failed("User ID không hợp lệ."));
                }
                addressDto.UserId = userId;
                await addressService.UpdateAsync(id, addressDto);
                return Ok(ApiResponse<AddressDto>.Succeeded(null!, "Cập nhật địa chỉ thành công!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Failed($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
