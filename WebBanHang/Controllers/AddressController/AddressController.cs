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
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null!);
            if (userId == null)
            {
                return BadRequest(ApiResponse<AddressDto>.Failed("User ID không hợp lệ."));
            }
            addressDto.UserId = userId;
            await addressService.AddAsync(addressDto);
            return Ok(ApiResponse<AddressDto>.Succeeded(null!, "Thêm địa chỉ thành công!", 201));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            await addressService.DeleteAsync(id);
            return Ok(ApiResponse<AddressDto>.Succeeded(null!, "Xóa địa chỉ thành công!"));
        }
    }
}
