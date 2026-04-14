using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.OrderController
{
    [Route("api/Admin/Orders")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrdersService _adminOrdersService;

        public AdminOrdersController(IAdminOrdersService adminOrdersService)
        {
            _adminOrdersService = adminOrdersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] AdminOrderQueryDto queryDto)
        {
            try
            {
                var result = await _adminOrdersService.GetOrdersAsync(queryDto);
                return Ok(ApiResponse<AdminOrderListResponseDto>.Succeeded(result, "Lấy danh sách đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _adminOrdersService.GetOrderDetailAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminOrderDetailDto>.Failed("Không tìm thấy đơn hàng.", 404));
            }
            return Ok(ApiResponse<AdminOrderDetailDto>.Succeeded(result, "Lấy chi tiết đơn hàng thành công"));
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] AdminUpdateOrderStatusDto dto)
        {
            try
            {
                var result = await _adminOrdersService.UpdateOrderStatusAsync(id, dto.Status);
                return Ok(ApiResponse<AdminOrderStatusResultDto>.Succeeded(result, "Cập nhật trạng thái đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }


        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            try
            {
                var result = await _adminOrdersService.UpdateOrderAsync(id, dto);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Cập nhật thông tin đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var result = await _adminOrdersService.DeleteOrderAsync(id);
                return Ok(ApiResponse<bool>.Succeeded(result, "Xóa đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CheckoutDto dto)
        {
            if (!dto.UserId.HasValue)
            {
                return BadRequest(ApiResponse<OrderDto>.Failed("Vui lòng cung cấp ID khách hàng.", 400));
            }

            try
            {
                var result = await _adminOrdersService.CreateOrderAsync(dto, dto.UserId.Value);
                return Ok(ApiResponse<OrderDto>.Succeeded(result, "Tạo đơn hàng mới thành công", 201));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
        }
    }
}
