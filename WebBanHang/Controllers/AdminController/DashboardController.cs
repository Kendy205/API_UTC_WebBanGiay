using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")] // Chỉ Admin mới có thể xem dashboard
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        /// Lấy thống kê tổng quan
        /// Query Parameters :
        /// - startDate: 2024-01-01
        /// - endDate: 2024-12-31
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryStatistics()
        {
            try
            {
                var result = await _dashboardService.GetSummaryStatisticsAsync();
                return Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy thống kê tổng quan tháng hiện tại thành công"
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
