using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> GetPayments([FromQuery] AdminPaymentQueryDto queryDto)
        {
            try
            {
                var result = await _adminPaymentsService.GetPaymentsAsync(queryDto);
                return Ok(ApiResponse<AdminPaymentListResponseDto>.Succeeded(result, "Lấy danh sách thanh toán thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }
    }
}
