using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.SizeController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly ISizeService _sizeService;
        public SizeController(ISizeService sizeService) => _sizeService = sizeService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _sizeService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sizeService.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SizeDto dto)
        {
            await _sizeService.AddAsync(dto);
            return Ok(new { message = "Thêm thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SizeDto dto)
        {
            await _sizeService.UpdateAsync(id, dto);
            return Ok(new { message = "Cập nhật thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _sizeService.DeleteAsync(id);
            return Ok(new { message = "Xóa thành công" });
        }
    }
}
