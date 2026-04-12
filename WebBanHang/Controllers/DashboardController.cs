using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.Controllers
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
        public async Task<IActionResult> GetSummaryStatistics(
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
                var result = await _dashboardService.GetSummaryStatisticsAsync(startDate, endDate);

                return base.Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy thống kê tổng quan thành công"
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

       

        
        [HttpGet("header-summary")]
        public async Task<IActionResult> GetHeaderSummary()
        {
            try
            {
                var result = await _dashboardService.GetHeaderSummaryAsync();

                return base.Ok(ApiResponse<object>.Succeeded(
                    result,
                    "Lấy tổng quan đầu ngày thành công"
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
