using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    public class AdminProductsController : ControllerBase
    {
        private readonly IAdminProductService _service;

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
        public async Task<IActionResult> Create([FromForm] ProductAdminDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return Ok(ApiResponse<ProductAdminDto>
                .Succeeded(result, "Tạo sản phẩm thành công!"));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductAdminDto dto)
        {
            await _service.UpdateAsync(id, dto);

            return Ok(ApiResponse<string>
                .Succeeded(null, "Cập nhật thành công!"));
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