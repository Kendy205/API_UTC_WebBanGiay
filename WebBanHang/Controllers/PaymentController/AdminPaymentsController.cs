using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Payment;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.PaymentController
{
    [Route("api/Admin/payments")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly IAdminPaymentService _adminPaymentService;

        public AdminPaymentsController(IAdminPaymentService adminPaymentService)
        {
            _adminPaymentService = adminPaymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] AdminPaymentQueryDto queryDto)
        {
            var result = await _adminPaymentService.GetPaymentsAsync(queryDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
