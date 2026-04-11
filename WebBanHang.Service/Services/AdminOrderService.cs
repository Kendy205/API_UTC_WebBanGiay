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

        public Task<ApiResponse<AdminOrderListResponseDto>> GetOrdersAsync(AdminOrderQueryDto queryDto)
        {
            return _orderService.GetAdminOrdersAsync(queryDto);
        }

        public Task<ApiResponse<AdminOrderDetailDto?>> GetOrderByIdAsync(long orderId)
        {
            return _orderService.GetAdminOrderDetailAsync(orderId);
        }

        public Task<ApiResponse<AdminOrderStatusResultDto>> UpdateOrderStatusAsync(long orderId, string status, long adminUserId)
        {
            return _orderService.AdminUpdateOrderStatusAsync(orderId, status, adminUserId);
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

        public Task<ApiResponse<bool>> DeleteOrderAsync(long orderId, long adminUserId)
        {
            return _orderService.DeleteAsync(orderId, adminUserId);
        }

    }
}
