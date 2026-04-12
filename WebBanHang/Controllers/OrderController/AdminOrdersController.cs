using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/Admin/orders")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !long.TryParse(claim.Value, out var userId))
            {
                return 0;
            }

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] AdminOrderQueryDto queryDto)
        {
            var result = await _adminOrderService.GetOrdersAsync(queryDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _adminOrderService.GetOrderByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] AdminUpdateOrderStatusDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _adminOrderService.UpdateOrderStatusAsync(id, dto.Status, currentUserId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _adminOrderService.CheckoutAsync(dto, currentUserId);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.OrderId }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            var result = await _adminOrderService.UpdateOrderAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _adminOrderService.DeleteOrderAsync(id, currentUserId);
            return StatusCode(result.StatusCode, result);
        }

    }
}
