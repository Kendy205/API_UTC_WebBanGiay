using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Common;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class ReportsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public ReportsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// Get sales report for a date range grouped by specified period
        /// <param name="from">Start date (ISO format: yyyy-MM-dd)</param>
        /// <param name="to">End date (ISO format: yyyy-MM-dd)</param>
        /// <param name="groupBy">Grouping option: "day", "week", or "month" (default: "day")</param>
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport(
     [FromQuery] DateTime? from,
     [FromQuery] DateTime? to,
     [FromQuery] string groupBy = "day")
        {
            try
            {
                // Validate null
                if (from == null || to == null)
                {
                    return BadRequest(ApiResponse<object>.Failed(
                        "Tham số 'from' và 'to' là bắt buộc",
                        400
                    ));
                }

                var fromDate = from.Value;
                var toDate = to.Value;

                // Validate logic
                if (fromDate > toDate)
                {
                    return BadRequest(ApiResponse<object>.Failed(
                        "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc",
                        400
                    ));
                }

                // Validate groupBy
                var validGroupBy = new[] { "day", "week", "month" };
                if (!validGroupBy.Contains(groupBy?.ToLower()))
                {
                    return BadRequest(ApiResponse<object>.Failed(
                        "Tham số 'groupBy' phải là: day, week, hoặc month",
                        400
                    ));
                }

                // Cover full ngày
                toDate = toDate.Date.AddDays(1).AddTicks(-1);

                var result = await _dashboardService.GetSalesReportAsync(
                    fromDate,
                    toDate,
                    groupBy.ToLower()
                );

                return Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy báo cáo doanh thu thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(
                    $"Lỗi server: {ex.Message}",
                    500
                ));
            }
        }
    }
}
