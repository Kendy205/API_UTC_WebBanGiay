using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var order = await _myOrdersService.GetMyOrderByIdAsync(id, userId);
            if(order.Success && order.Data != null)
            {
                if (order.Data.PaymentStatus == "Paid" || order.Data.PaymentStatus == "Success")
                {
                    return BadRequest(ApiResponse<string>.Failed("Không thể hủy đơn hàng đã thanh toán!", 400));
                }
            }
            var result = await _myOrdersService.CancelMyOrderAsync(id, userId);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("{orderId}/pay-vnpay")]
        public async Task<IActionResult> GetVnPayUrl(long orderId)
        {
            // 1. Kiểm tra xem đơn hàng có tồn tại và có thuộc về user đang đăng nhập không
            var orderResult = await _myOrdersService.GetMyOrderByIdAsync(orderId, long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));

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
            return Ok(ApiResponse<string>.Succeeded(url, "Tạo link thanh toán VNPay thành công."));
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
