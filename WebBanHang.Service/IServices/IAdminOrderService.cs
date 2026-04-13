using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IAdminOrderService
    {
        Task<AdminOrderListResponseDto> GetOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderDetailDto> GetOrderByIdAsync(long orderId);
        Task<AdminOrderStatusResultDto> UpdateOrderStatusAsync(long orderId, string status, long adminUserId);
        Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long adminUserId);
        Task<OrderDto> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto);
        Task DeleteOrderAsync(long orderId, long adminUserId);
    }
}
