using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers.ColorController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _colorService;
        public ColorController(IColorService colorService) => _colorService = colorService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _colorService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _colorService.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ColorDto dto)
        {
            await _colorService.AddAsync(dto);
            return Ok(new { message = "Thêm thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ColorDto dto)
        {
            await _colorService.UpdateAsync(id, dto);
            return Ok(new { message = "Cập nhật thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _colorService.DeleteAsync(id);
            return Ok(new { message = "Xóa thành công" });
        }
    }
}