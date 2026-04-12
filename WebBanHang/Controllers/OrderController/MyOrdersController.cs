using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/my/orders")]
    [ApiController]
    [Authorize]
    public class MyOrdersController : ControllerBase
    {
        private readonly IMyOrderService _myOrderService;

        public MyOrdersController(IMyOrderService myOrderService)
        {
            _myOrderService = myOrderService;
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
        public async Task<IActionResult> GetMyOrders()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _myOrderService.GetMyOrdersAsync(currentUserId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetMyOrderById(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _myOrderService.GetMyOrderByIdAsync(id, currentUserId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _myOrderService.CheckoutAsync(dto, currentUserId);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return CreatedAtAction(nameof(GetMyOrderById), new { id = result.Data?.OrderId }, result);
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Unauthorized", 401));

            var result = await _myOrderService.CancelMyOrderAsync(id, currentUserId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
