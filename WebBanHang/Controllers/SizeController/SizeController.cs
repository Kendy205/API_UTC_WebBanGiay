using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.SizeController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly ISizeService _sizeService;
        public SizeController(ISizeService sizeService) => _sizeService = sizeService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _sizeService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<SizeDto>>.Succeeded(result, "Lấy danh sách kích thước thành công!"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sizeService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<SizeDto>.Failed("Kích thước không tồn tại!", 404));

            return Ok(ApiResponse<SizeDto>.Succeeded(result , "Lấy kích thước thành công!"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SizeDto dto)
        {
            await _sizeService.AddAsync(dto);
            return Ok(ApiResponse<SizeDto>.Succeeded(dto, "Thêm kích thước thành công!"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SizeDto dto)
        {
            var existing = await _sizeService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Kích thước không tồn tại!", 404));

            await _sizeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<SizeDto>.Succeeded(dto, "Cập nhật kích thước thành công!"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _sizeService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Kích thước không tồn tại!", 404));

            await _sizeService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Xóa kích thước thành công!"));
        }
    }
}