using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.Admin
{
    [Route("api/Admin/brands")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminBrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public AdminBrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

    
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _brandService.GetAllByAdminAsync();

            return Ok(ApiResponse<IEnumerable<BrandDto>>
                .Succeeded(result, "Lấy danh sách thương hiệu thành công!"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _brandService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<string>.Failed("Thương hiệu không tồn tại!", 404));
            }

            return Ok(ApiResponse<BrandDto>
                .Succeeded(result, "Lấy chi tiết thương hiệu thành công!"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandDto dto)
        {
            try
            {
                await _brandService.AddAsync(dto);
                return Ok(ApiResponse<BrandDto>.Succeeded(dto, "Thêm thương hiệu thành công!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed($"Lỗi: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] BrandDto dto)
        {
            if (id != dto.BrandId)
            {
                return BadRequest(ApiResponse<string>.Failed("ID thương hiệu không khớp!"));
            }

            var existing = await _brandService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Failed("Thương hiệu không tồn tại!", 404));
            }

            try
            {
                await _brandService.UpdateAsync(id, dto);
                return Ok(ApiResponse<BrandDto>.Succeeded(dto, "Cập nhật thương hiệu thành công!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed($"Lỗi: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _brandService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Failed("Thương hiệu không tồn tại!", 404));
            }

            await _brandService.DeleteAsync(id);

            return Ok(ApiResponse<string>
                .Succeeded(null, "Xóa thương hiệu thành công!"));
        }
    }
}