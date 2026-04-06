using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ProductController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductVariantService _variantService;

        public ProductController(IProductService productService, IProductVariantService variantService)
        {
            _productService = productService;
            _variantService = variantService;
        }

        // GET: api/product
        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Succeeded(products, "Lấy danh sách sản phẩm thành công!"));
        }

        // GET: api/product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var product = await _productService.GetByIdAsync(id);
            product.Variants = (List<ProductVariantDto>?)await _variantService.GetByProductIdAsync(id); // Lấy biến thể cho sản phẩm
            if (product == null)
                return NotFound(ApiResponse<ProductDto>.Failed("Không tìm thấy sản phẩm!", 404));

            return Ok(ApiResponse<ProductDto>.Succeeded(product, "Lấy sản phẩm thành công!"));
        }

        // POST: api/product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            await _productService.AddAsync(dto);
            // Thường sau khi tạo, ta trả về chính object đó hoặc chỉ cần message thành công
            return Ok(ApiResponse<ProductDto>.Succeeded(dto, "Thêm sản phẩm thành công!"));
        }

        // PUT: api/product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductDto dto)
        {
            if (id != dto.ProductId)
            {
                return BadRequest(ApiResponse<string>.Failed("ID sản phẩm không khớp!"));
            }

            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));

            await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.Succeeded(dto, "Cập nhật sản phẩm thành công!"));
        }

        // DELETE: api/product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));

            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Succeeded(null, "Xóa sản phẩm thành công!"));
        }
    }
}