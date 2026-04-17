using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Payment;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.PaymentController
{
    [Route("api/Admin/Payments")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly IAdminPaymentsService _adminPaymentsService;

        public AdminPaymentsController(IAdminPaymentsService adminPaymentsService)
        {
            _adminPaymentsService = adminPaymentsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments(
                [FromQuery] string? status,
                [FromQuery] string? method,
                [FromQuery] string? search,
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endDate,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            try
            {
                // 1. Gọi Service lấy dữ liệu thô (DTO)
                var result = await _adminPaymentsService.GetPaymentsAsync(
                    status, method, search, startDate, endDate, page, pageSize);


                return Ok(ApiResponse<AdminPaymentListResponseDto>.Succeeded(result, "Lấy danh sách thanh toán thành công"));
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }
    }
}
