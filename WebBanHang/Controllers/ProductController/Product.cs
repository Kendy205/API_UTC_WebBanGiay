using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ProductController
{
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productServicel;

        public ProductController(IUnitOfWork unitOfWork , IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productServicel = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await _productServicel.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Succeeded(products));    
        }
    }
}
