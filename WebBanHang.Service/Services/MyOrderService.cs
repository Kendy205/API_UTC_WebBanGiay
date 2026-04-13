using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class MyOrderService : IMyOrderService
    {
        private readonly IOrderService _orderService;

        public MyOrderService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long currentUserId)
        {
            return _orderService.GetOrdersAsync(currentUserId, false);
        }

        public Task<OrderDto> GetMyOrderByIdAsync(long orderId, long currentUserId)
        {
            return _orderService.GetByIdAsync(orderId, currentUserId, false);
        }

        public Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId)
        {
            // User checkout luôn đặt cho chính mình, không dùng UserId từ request.
            checkoutDto.UserId = null;
            return _orderService.PlaceOrderAsync(checkoutDto, currentUserId);
        }

        public Task CancelMyOrderAsync(long orderId, long currentUserId)
        {
            return _orderService.CancelOrderAsync(orderId, currentUserId);
        }
    }
}
