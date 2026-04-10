using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IOrderService _orderService;

        public AdminOrderService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Task<ApiResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync()
        {
            return _orderService.GetOrdersAsync(0, true);
        }

        public Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(long orderId)
        {
            return _orderService.GetByIdAsync(orderId, 0, true);
        }

        public Task<ApiResponse<OrderDto>> CheckoutAsync(CheckoutDto checkoutDto, long adminUserId)
        {
            var targetUserId = checkoutDto.UserId ?? adminUserId;
            return _orderService.PlaceOrderAsync(checkoutDto, targetUserId);
        }

        public Task<ApiResponse<OrderDto>> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto)
        {
            return _orderService.AdminUpdateOrderAsync(orderId, updateDto);
        }

        public Task<ApiResponse<bool>> DeleteOrderAsync(long orderId)
        {
            return _orderService.DeleteAsync(orderId);
        }
    }
}
