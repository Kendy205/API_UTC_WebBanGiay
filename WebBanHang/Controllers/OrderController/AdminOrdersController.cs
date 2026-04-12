using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.Exceptions;
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
            try
            {
                var result = await _adminOrderService.GetOrdersAsync(queryDto);
                return Ok(ApiResponse<AdminOrderListResponseDto>.Succeeded(result));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<AdminOrderListResponseDto>(ex);
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var result = await _adminOrderService.GetOrderByIdAsync(id);
                return Ok(ApiResponse<AdminOrderDetailDto>.Succeeded(result));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<AdminOrderDetailDto>(ex);
            }
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] AdminUpdateOrderStatusDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                var result = await _adminOrderService.UpdateOrderStatusAsync(id, dto.Status, currentUserId);
                return Ok(ApiResponse<AdminOrderStatusResultDto>.Succeeded(result, "Cập nhật trạng thái đơn hàng thành công!"));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<AdminOrderStatusResultDto>(ex);
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                var result = await _adminOrderService.CheckoutAsync(dto, currentUserId);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Đặt hàng thành công!", 201));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<OrderDto>(ex);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            try
            {
                var result = await _adminOrderService.UpdateOrderAsync(id, dto);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Cập nhật đơn hàng thành công."));
            }
            catch (ApiException ex)
            {
                return BuildErrorResponse<OrderDto>(ex);
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập.", 401));

            try
            {
                await _adminOrderService.DeleteOrderAsync(id, currentUserId);
                return Ok(ApiResponse<bool>.Succeeded(true, "Xóa đơn hàng thành công."));
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
