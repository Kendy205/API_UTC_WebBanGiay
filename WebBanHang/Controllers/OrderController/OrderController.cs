using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.DTOs.Common;
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
        private readonly IPaymentService _paymentService;

        public OrderController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
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

            if (IsAdmin() && dto.UserId.HasValue)
            {
                targetUserId = dto.UserId.Value;
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
        [HttpDelete("delete")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteOrder([FromBody] DeleteOrderDto dto)
        {
            var result = await _orderService.DeleteAsync(dto.OrderId);
            if (!result.Success) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
        /// <summary>
        /// API dành cho Frontend gọi để lấy Link thanh toán VNPay
        /// </summary>
        [HttpPost("{orderId}/pay-vnpay")]
        public async Task<IActionResult> GetVnPayUrl(long orderId)
        {
            // 1. Kiểm tra xem đơn hàng có tồn tại và có thuộc về user đang đăng nhập không
            var orderResult = await _orderService.GetByIdAsync(orderId, GetCurrentUserId(), IsAdmin());

            if (!orderResult.Success || orderResult.Data == null)
            {
                return NotFound(orderResult); // Lỗi 404 nếu không tìm thấy
            }

            // 2. Chặn lại nếu đơn hàng đã được thanh toán rồi
            if (orderResult.Data.PaymentStatus == "Paid" || orderResult.Data.PaymentStatus == "Success")
            {
                return BadRequest(new { success = false, message = "Đơn hàng này đã được thanh toán trước đó." });
            }

            // 3. Sinh link VNPay bằng PaymentService
            // Truyền OrderDto và HttpContext (để lấy IP người dùng) sang Service
            string url = _paymentService.CreateVnPayPaymentUrl(orderResult.Data, HttpContext);

            // 4. Trả link về cho React
            //return Ok(new
            //{
            //    success = true,
            //    paymentUrl = url,
            //    message = "Tạo link thanh toán VNPay thành công."
            //});
            return Ok(ApiResponse<string>.Succeeded(url,"Tạo link thanh toán VNPay thành công."));
        }
        [HttpGet("vnpay-return")]
        [AllowAnonymous] // VNPay gọi ngầm vào đây nên không bắt Auth nhé
        public async Task<IActionResult> VnPayReturn()
        {
            // 1. Gọi Service để Verify chữ ký và Cập nhật DB
            // Service sẽ tự kiểm tra mã 00 và đổi PaymentStatus thành Paid
            var response = await _paymentService.ProcessVnPayReturn(Request.Query);

            // 2. Lấy toàn bộ chuỗi query params gốc từ VNPay (VD: ?vnp_Amount=500000&vnp_ResponseCode=00...)
            var queryString = Request.QueryString.Value;

            // 3. Redirect về React (Port 5173 của Vite) kèm theo query params
            return Redirect($"http://localhost:5173/vnpay-return{queryString}");
        }
    }
}
