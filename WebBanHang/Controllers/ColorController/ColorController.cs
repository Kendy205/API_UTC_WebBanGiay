using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common; // Thêm namespace này
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ColorController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _colorService;
        public ColorController(IColorService colorService) => _colorService = colorService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _colorService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ColorDto>>.Succeeded(result, "Lấy danh sách màu sắc thành công!"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _colorService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<ColorDto>.Failed("Không tìm thấy màu sắc!", 404));

            return Ok(ApiResponse<ColorDto>.Succeeded(result, "Lấy màu sắc thành công!"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ColorDto dto)
        {
            await _colorService.AddAsync(dto);
            return Ok(ApiResponse<ColorDto>.Succeeded(dto, "Thêm màu sắc thành công!"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ColorDto dto)
        {
            var existing = await _colorService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Màu sắc không tồn tại!", 404));

            await _colorService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ColorDto>.Succeeded(dto, "Cập nhật màu sắc thành công!"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _colorService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Màu sắc không tồn tại!", 404));

            await _colorService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Xóa màu sắc thành công!"));
        }
    }
}