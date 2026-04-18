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
using WebBanHang.Service.Services;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/My/Orders")]
    [ApiController]
    [Authorize]
    public class MyOrdersController : ControllerBase
    {
        private readonly IMyOrdersService _myOrdersService;
        private readonly IPaymentService _paymentService;

        public MyOrdersController(IMyOrdersService myOrdersService, IPaymentService paymentService)
        {
            _myOrdersService = myOrdersService;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            if (userId == 0) return Unauthorized(ApiResponse<object>.Failed("Vui lòng đăng nhập", 401));

            var result = await _myOrdersService.GetMyOrdersAsync(userId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<OrderDto>>.Succeeded(result, "Lấy danh sách đơn hàng thành công"));
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

            var order = await _myOrdersService.GetMyOrderByIdAsync(id, userId);
            if (order == null)
            {
                return NotFound(ApiResponse<string>.Failed("Đơn hàng không tồn tại.", 404));
            }

            if (order.PaymentStatus == "Paid" || order.PaymentStatus == "Success")
            {
                return BadRequest(ApiResponse<string>.Failed("Không thể hủy đơn hàng đã thanh toán!", 400));
            }

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
        [HttpPost("{orderId}/pay-vnpay")]
        public async Task<IActionResult> GetVnPayUrl(long orderId)
        {
            // 1. Kiểm tra xem đơn hàng có tồn tại và có thuộc về user đang đăng nhập không
            var orderResult = await _myOrdersService.GetMyOrderByIdAsync(orderId, long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));

            if (orderResult == null)
            {
                return NotFound(orderResult); // Lỗi 404 nếu không tìm thấy
            }

            // 2. Chặn lại nếu đơn hàng đã được thanh toán rồi
            if (orderResult.PaymentStatus == "Paid" || orderResult.PaymentStatus == "Success")
            {
                return BadRequest(new { success = false, message = "Đơn hàng này đã được thanh toán trước đó." });
            }

            // 3. Sinh link VNPay bằng PaymentService
            // Truyền OrderDto và HttpContext (để lấy IP người dùng) sang Service
            string url = _paymentService.CreateVnPayPaymentUrl(orderResult, HttpContext);

            // 4. Trả link về cho React
            //return Ok(new
            //{
            //    success = true,
            //    paymentUrl = url,
            //    message = "Tạo link thanh toán VNPay thành công."
            //});
            return Ok(ApiResponse<string>.Succeeded(url, "Tạo link thanh toán VNPay thành công."));
        }
        [HttpGet("vnpay-return")]
        [AllowAnonymous] // VNPay gọi ngầm vào đây nên không bắt Auth nhé
        public async Task<IActionResult> VnPayReturn()
        {
           
            try
            {
                // 1. Gọi Service xử lý DB
                var response = await _paymentService.ProcessVnPayReturn(Request.Query);

                if (response)
                {
                    // 2. BẮT BUỘC trả về JSON đúng format này cho VNPay
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    return Ok(new { RspCode = "97", Message = "Invalid Signature or Failed" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }
    }
}