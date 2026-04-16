using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.UserController
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.Succeeded(result));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserResgiterDto userCreateDto)
        {
            try
            {
                 await _userService.AddAsync(userCreateDto);
                return Ok(ApiResponse<string>.Succeeded("Tạo người dùng thành công!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message, 400));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UserResgiterDto userUpdateDto)
        {
            if(id <= 0)
            {
                return BadRequest(ApiResponse<string>.Failed("ID không hợp lệ!", 400));
            }
            try
            {
                await _userService.UpdateAsync(id, userUpdateDto);
                return Ok(ApiResponse<string>.Succeeded("Cập nhật người dùng thành công!"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.Failed("Người dùng không tồn tại!", 404));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message, 400));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return Ok(ApiResponse<string>.Succeeded("Xóa người dùng thành công!"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.Failed("Người dùng không tồn tại!", 404));
            }
        }
    }
}
