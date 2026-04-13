using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _adminOrdersService.GetOrdersAsync(queryDto);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _adminOrdersService.GetOrderDetailAsync(id);
            if (!result.Success)
            {
                return result.StatusCode == 404 
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] AdminUpdateOrderStatusDto dto)
        {
            var result = await _adminOrdersService.UpdateOrderStatusAsync(id, dto.Status);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            var result = await _adminOrdersService.UpdateOrderAsync(id, dto);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _adminOrdersService.DeleteOrderAsync(id);
            if (!result.Success)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CheckoutDto dto)
        {
            if (!dto.UserId.HasValue)
            {
                return BadRequest(ApiResponse<OrderDto>.Failed("Vui lòng cung cấp ID khách hàng.", 400));
            }

            var result = await _adminOrdersService.CreateOrderAsync(dto, dto.UserId.Value);
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
