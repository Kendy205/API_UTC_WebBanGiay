using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ProductVariantController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;
        public ProductVariantController(IProductVariantService variantService) => _variantService = variantService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _variantService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.Succeeded(result, "Lấy danh sách sản phẩm thành công!"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _variantService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<ProductVariantDto>.Failed("Sản phẩm không tồn tại!", 404));

            return Ok(ApiResponse<ProductVariantDto>.Succeeded(result, "Lấy sản phẩm thành công!"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductVariantDto dto)
        {
            await _variantService.AddAsync(dto);
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(dto, "Thêm sản phẩm thành công!"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductVariantDto dto)
        {
            var existing = await _variantService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));

            await _variantService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(dto, "Cập nhật sản phẩm thành công!"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _variantService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));

            await _variantService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Xóa sản phẩm nthành công!"));
        }
    }
}
