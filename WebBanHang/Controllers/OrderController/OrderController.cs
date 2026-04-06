using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.DTOs.Common;
using System.Security.Claims;
using System.Linq;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? long.Parse(claim.Value) : 0;
        }

        private bool IsAdmin() => User.IsInRole("1");

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var result = await _orderService.GetOrdersAsync(GetCurrentUserId(), IsAdmin());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _orderService.GetByIdAsync(id, GetCurrentUserId(), IsAdmin());
            if (!result.Success) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> PlaceOrder([FromBody] CheckoutDto dto)
        {
            long targetUserId = GetCurrentUserId();

            if (IsAdmin() && dto.CustomerId.HasValue)
            {
                targetUserId = dto.CustomerId.Value;
            }

            var result = await _orderService.PlaceOrderAsync(dto, targetUserId);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.OrderId }, result);
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderDto dto)
        {
            long? ownerId = IsAdmin() ? null : GetCurrentUserId();
            var result = await _orderService.CancelOrderAsync(dto.OrderId, ownerId);
            if (!result.Success) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        // ── ADMIN ONLY ACTIONS ────────────────────────────────

        /// <summary>
        /// Admin cập nhật thông tin đơn hàng (ID nằm trong Body).
        /// </summary>
        [HttpPut("admin-update")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AdminUpdateOrder([FromBody] OrderUpdateDto dto)
        {
            var result = await _orderService.AdminUpdateOrderAsync(dto.OrderId, dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Admin xóa đơn hàng vĩnh viễn (ID nằm trong Body).
        /// </summary>
        [HttpPost("delete")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteOrder([FromBody] DeleteOrderDto dto)
        {
            var result = await _orderService.DeleteAsync(dto.OrderId);
            if (!result.Success) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
    }
}
