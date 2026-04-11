using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;
using WebBanHang.Service.Services;

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
        //[HttpPost]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> Create([FromBody] ProductDto dto)
        //{
        //    await _productService.AddAsync(dto);
        //    return Ok(ApiResponse<ProductDto>.Succeeded(dto, "Thêm sản phẩm thành công!"));
        //}
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto, IFormFile file)
        {
            try
            {
                var result = await _productService.AddAsync(dto, file);

                //if (result.Error != null) return BadRequest(result.Error.Message);

                return Ok(ApiResponse<ProductDto>.Succeeded(result, "Thêm sản phẩm thành công!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed($"Lỗi: {ex.Message}"));
            }

        }

        // PUT: api/product/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
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
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));

            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Succeeded(null, "Xóa sản phẩm thành công!"));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] string? keyword, [FromQuery] long? categoryId, [FromQuery] long? brandId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int validPageNumber = pageNumber > 0 ? pageNumber : 1;
            int validPageSize = pageSize > 0 ? pageSize : 10;

            var products = await _productService.GetFilteredProductsAsync(
                keyword, categoryId, brandId, minPrice, maxPrice, validPageNumber, validPageSize);

            return Ok(ApiResponse<IEnumerable<ProductDto>>.Succeeded(products, "Lấy danh sách sản phẩm thành công!"));
        }
    }
}