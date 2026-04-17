using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.InventoryController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryMovementService _inventoryService;

        public InventoryController(IInventoryMovementService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll(int page, int pageSize)
        //{
        //    //var movements = await _inventoryService.GetAllAsync(page, pageSize);
        //   // return Ok(movements);
        //}
    }
}
