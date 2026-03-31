using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.BLL.Services;
using WebBanHang.DTOs.Common;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ProductController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;

        public ProductController(IUnitOfWork unitOfWork, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Succeeded(products));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(long id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }
        [HttpPost]
        public async Task<ActionResult> Create(ProductDto dto)
        {
            await _productService.AddAsync(dto);
            return Ok(new { message = "Created successfully" });
        }

        // PUT: api/product/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(long id, ProductDto dto)
        {
            if (id != dto.ProductId)
            {
                return BadRequest();
            }
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Product not found" });

            await _productService.UpdateAsync(id, dto);
            return Ok(new { message = "Updated successfully" });
        }

        // DELETE: api/product/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Product not found" });

            await _productService.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
