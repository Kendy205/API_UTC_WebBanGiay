using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Succeeded(result, "Lấy danh sách đơn hàng thành công"));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetMyOrderById(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.GetMyOrderByIdAsync(id, userId);
            if (result == null)
            {
                return NotFound(ApiResponse<OrderDto>.Failed("Đơn hàng không tồn tại.", 404));
            }
            return Ok(ApiResponse<OrderDto>.Succeeded(result, "Lấy chi tiết đơn hàng thành công"));
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            try
            {
                var result = await _myOrdersService.CheckoutAsync(dto, userId);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Đặt hàng thành công", 201));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            try
            {
                var result = await _myOrdersService.CancelMyOrderAsync(id, userId);
                return Ok(ApiResponse<bool>.Succeeded(result, "Hủy đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }
    }
}
