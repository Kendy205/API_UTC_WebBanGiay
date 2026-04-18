using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

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

        [HttpGet]
        public async Task<IActionResult> GetAllProduct(int? pageSize,int page)
        {
            var result = await _productService.GetAllAsync(pageSize,page);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Succeeded(result, "Lấy danh sách sản phẩm thành công!"));
        }
        //[HttpGet]
        //public async Task<IActionResult> GetAllProductFromBrand(int? pageSize, int page, long brandId)
        //{
        //    var result = await _productService.GetAllAsync(pageSize, page, brandId);
        //    return Ok(ApiResponse<PagedResult<ProductDto>>.Succeeded(result, "Lấy danh sách sản phẩm theo thương hiệu thành công!"));
        //}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.Failed("Không tìm thấy sản phẩm!", 404));
            }

            product.Variants = (List<ProductVariantDto>?)await _variantService.GetByProductIdAsync(id);
            return Ok(ApiResponse<ProductDto>.Succeeded(product, "Lấy sản phẩm thành công!"));
        }



        //[HttpPut("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> Update(long id, [FromBody] ProductDto dto)
        //{
        //    if (id != dto.ProductId)
        //    {
        //        return BadRequest(ApiResponse<string>.Failed("ID sản phẩm không khớp!"));
        //    }

        //    var existing = await _productService.GetByIdAsync(id);
        //    if (existing == null)
        //    {
        //        return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));
        //    }

        //    await _productService.UpdateAsync(id, dto);
        //    return Ok(ApiResponse<ProductDto>.Succeeded(dto, "Cập nhật sản phẩm thành công!"));
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    var existing = await _productService.GetByIdAsync(id);
        //    if (existing == null)
        //    {
        //        return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));
        //    }

        //    await _productService.DeleteAsync(id);
        //    return Ok(ApiResponse<string>.Succeeded(null, "Xóa sản phẩm thành công!"));
        //}

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] string? keyword, [FromQuery] long? categoryId, [FromQuery] long? brandId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? sortBy = "newest")
        {
            var validPageNumber = pageNumber > 0 ? pageNumber : 1;
            var validPageSize = pageSize > 0 ? pageSize : 8;
            var result = await _productService.GetFilteredProductsAsync(
                keyword,
                categoryId,
                brandId,
                minPrice,
                maxPrice,
                validPageNumber,
                validPageSize,
                sortBy
                );
            return Ok(ApiResponse<PagedResult<ProductDto>>.Succeeded(result, "Lấy danh sách sản phẩm thành công!"));
        }

    }
}
