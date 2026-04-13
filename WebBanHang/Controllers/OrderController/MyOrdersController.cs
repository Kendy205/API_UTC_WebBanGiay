using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.Exceptions;
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
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                var result = await _myOrderService.GetMyOrdersAsync(currentUserId);
                return Ok(ApiResponse<IEnumerable<OrderDto>>.Succeeded(result));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<IEnumerable<OrderDto>>(ex);
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetMyOrderById(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                var result = await _myOrderService.GetMyOrderByIdAsync(id, currentUserId);
                return Ok(ApiResponse<OrderDto>.Succeeded(result));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<OrderDto>(ex);
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                var result = await _myOrderService.CheckoutAsync(dto, currentUserId);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Đặt hàng thành công!", 201));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<OrderDto>(ex);
            }
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                await _myOrderService.CancelMyOrderAsync(id, currentUserId);
                return Ok(ApiResponse<bool>.Succeeded(true, "Hủy đơn hàng thành công."));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<bool>(ex);
            }
        }

        private IActionResult BuildErrorResponse<T>(ApiException ex)
        {
            var response = ApiResponse<T>.Failed(ex.Message, ex.StatusCode);
            return ex.StatusCode switch
            {
                401 => Unauthorized(response),
                404 => NotFound(response),
                403 => StatusCode(403, response),
                _ => BadRequest(response)
            };
        }
    }
}
