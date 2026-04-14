using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Common;

namespace WebBanHang.Controllers.DashboardController.AdminController
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
        public async Task<IActionResult> GetAnalyticsOverview([FromQuery] int year)
        {
            try
            {
                var result = await _dashboardService.GetAnalyticsOverviewAsync(year);

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

        /// Lấy phân bổ trạng thái đơn hàng
        /// Query Parameters :
        /// - startDate: 2024-01-01
        /// - endDate: 2024-12-31
        [HttpGet("orders/status-distribution")]
        public async Task<IActionResult> GetOrderStatusDistribution(
            [FromQuery] int year)
        {
            try
            {

                //  Gọi service
                var result = await _dashboardService.GetOrderStatusDistributionAsync(year);

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
