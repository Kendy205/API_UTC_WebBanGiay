using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;
using WebBanHang.Service.Services;

namespace WebBanHang.Controllers.Admin
{
    [Route("api/Admin/products")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IAdminProductService _service;
            //private readonly IProductService _productService;

        public AdminProductsController(IAdminProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 10, string? search = null)
        {
            var result = await _service.GetProductsAsync(search, page, pageSize);

            return Ok(ApiResponse<PagedResult<ProductAdminDto>>
                .Succeeded(result, "Lấy danh sách sản phẩm thành công!"));
        }

        [HttpPost]
        //public async Task<IActionResult> Create([FromForm] ProductAdminDto dto, IFormFile file)
        //{
        //    var result = await _service.CreateAsync(dto, file);

        //    return Ok(ApiResponse<ProductAdminDto>
        //        .Succeeded(result, "Tạo sản phẩm thành công!"));
        //}
      
        
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto, IFormFile file)
        {
            try
            {
                var result = await _service.AddAsync(dto, file);
                return Ok(ApiResponse<ProductDto>.Succeeded(result, "Thêm sản phẩm thành công!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed($"Lỗi: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromForm] ProductDto dto, IFormFile? file) // Thêm IFormFile
        {
            if (id != dto.ProductId)
            {
                return BadRequest(ApiResponse<string>.Failed("ID sản phẩm không khớp!"));
            }

            var existing = await _service.GetProductByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Failed("Sản phẩm không tồn tại!", 404));
            }

            // Truyền cả file vào hàm Update của Service
            var result = await _service.UpdateAsync(id, dto, file);

            return Ok(ApiResponse<ProductDto>.Succeeded(result, "Cập nhật sản phẩm thành công!"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);

            return Ok(ApiResponse<string>
                .Succeeded(null, "Xóa thành công!"));
        }
    }
}