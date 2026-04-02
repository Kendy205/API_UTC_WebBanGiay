using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ProductVariantController
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductVariantController(IProductVariantService _productVariantService) : Controller
    {

        [HttpGet("{productId}")]
        public async Task<IActionResult> getProductVariant(int productId)
        {
            var result =await _productVariantService.GetByProductIdAsync(productId);
            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.Succeeded(result));
        }
    }
}
