using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.CategoryController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService _categoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.Succeeded(result));
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            await _categoryService.AddAsync(dto);
            return Ok(ApiResponse<CategoryDto>.Succeeded(dto, "Thêm danh mục thành công!", 201));
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] CategoryDto dto)
        {
            if(id != dto.CategoryId)
            {
                return BadRequest(ApiResponse<string>.Failed("ID trong URL không khớp với ID trong body!", 400));
            }
            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Failed("Danh mục không tồn tại!", 404));
            }
            await _categoryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CategoryDto>.Succeeded(dto, "Cập nhật danh mục thành công!"));
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Failed("Danh mục không tồn tại!", 404));
            }
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Succeeded("Xóa danh mục thành công!"));

        }
    }
}
