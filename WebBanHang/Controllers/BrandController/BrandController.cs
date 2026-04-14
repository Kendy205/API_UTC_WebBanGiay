using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.BrandController
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController(IBrandService _brandService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _brandService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<BrandDto>>.Succeeded(result));
        }
    }
}
