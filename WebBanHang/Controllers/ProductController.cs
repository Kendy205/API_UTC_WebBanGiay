using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductController(IProductService _productService) : ControllerBase
    {
        [HttpGet]
        public  async Task<IActionResult> GetAllProduct()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Succeeded(products));
        }
    }
}
