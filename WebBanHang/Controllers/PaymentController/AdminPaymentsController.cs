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
        public async Task<IActionResult> GetPayments([FromQuery] AdminPaymentQueryDto queryDto)
        {
            var result = await _adminPaymentsService.GetPaymentsAsync(queryDto);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }
    }
}
