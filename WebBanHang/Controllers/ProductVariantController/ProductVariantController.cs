using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
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
        public async Task<IActionResult> GetAll() => Ok(await _variantService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _variantService.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductVariantDto dto)
        {
            await _variantService.AddAsync(dto);
            return Ok(new { message = "Thêm biến thể thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ProductVariantDto dto)
        {
            await _variantService.UpdateAsync(id, dto);
            return Ok(new { message = "Cập nhật biến thể thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _variantService.DeleteAsync(id);
            return Ok(new { message = "Xóa biến thể thành công" });
        }
    }
}
