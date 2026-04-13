using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/My/Orders")]
    [ApiController]
    [Authorize]
    public class MyOrdersController : ControllerBase
    {
        private readonly IMyOrdersService _myOrdersService;

        public MyOrdersController(IMyOrdersService myOrdersService)
        {
            _myOrdersService = myOrdersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.GetMyOrdersAsync(userId);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetMyOrderById(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.GetMyOrderByIdAsync(id, userId);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.CheckoutAsync(dto, userId);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.CancelMyOrderAsync(id, userId);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }
    }
}
