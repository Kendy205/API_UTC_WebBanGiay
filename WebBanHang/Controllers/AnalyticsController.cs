using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AnalyticsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// Get analytics overview with total revenue, orders, new customers, and status breakdown
        [HttpGet("overview")]
        public async Task<IActionResult> GetAnalyticsOverview()
        {
            try
            {
                var result = await _dashboardService.GetAnalyticsOverviewAsync();

                return Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy tổng quan phân tích thành công"
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


        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                // Validate parameters
                if (from.Equals(null) || to.Equals(null))
                {
                    return BadRequest(ApiResponse<object>.Failed(
                        "Tham số 'from' và 'to' là bắt buộc",
                        400
                    ));
                }


                if (from > to)
                {
                    return BadRequest(ApiResponse<object>.Failed(
                        "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc",
                        400
                    ));
                }

                // Set time to cover full days
                to = to.AddHours(23).AddMinutes(59).AddSeconds(59);

                var result = await _dashboardService.GetRevenueChartAsync(from, to);

                return Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy biểu đồ doanh thu thành công"
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


        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesStatistics(
            [FromQuery] string groupBy,
            [FromQuery] int year)
        {
            //  Validate input
            if (string.IsNullOrWhiteSpace(groupBy))
                return base.BadRequest(ApiResponse<object>.Failed(
                    "groupBy là bắt buộc (MONTH, QUARTER, hoặc YEAR)",
                    400
                ));

            if (year < 2000 || year > DateTime.UtcNow.Year + 10)
                return base.BadRequest(ApiResponse<object>.Failed(
                    "Năm phải từ 2000 đến hiện tại + 10 năm",
                    400
                ));

            try
            {
                //  Gọi service
                var result = await _dashboardService.GetSalesStatisticsAsync(groupBy, year);

                return base.Ok(ApiResponse<object>.Succeeded(
                    result,
                    $"Lấy thống kê doanh số theo {groupBy.ToLower()} năm {year} thành công"
                ));
            }
            catch (ArgumentException ex)
            {
                return base.BadRequest(ApiResponse<object>.Failed(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, ApiResponse<object>.Failed(
                    $"Lỗi server: {ex.Message}",
                    500
                ));
            }
        }

        /// Lấy phân bổ trạng thái đơn hàng
        /// Query Parameters :
        /// - startDate: 2024-01-01
        /// - endDate: 2024-12-31
        [HttpGet("orders/status-distribution")]
        public async Task<IActionResult> GetOrderStatusDistribution(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                //  Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                    return base.BadRequest(ApiResponse<object>.Failed(
                        "startDate phải nhỏ hơn hoặc bằng endDate",
                        400
                    ));
                //  Gọi service
                var result = await _dashboardService.GetOrderStatusDistributionAsync(startDate, endDate);

                return base.Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy phân bổ trạng thái đơn hàng thành công"
                ));
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, ApiResponse<object>.Failed(
                    $"Lỗi server: {ex.Message}",
                    500
                ));
            }
        }
    }
}
