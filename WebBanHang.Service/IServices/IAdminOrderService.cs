using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IAdminOrderService
    {
        Task<ApiResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync();
        Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(long orderId);
        Task<ApiResponse<OrderDto>> CheckoutAsync(CheckoutDto checkoutDto, long adminUserId);
        Task<ApiResponse<OrderDto>> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto);
        Task<ApiResponse<bool>> DeleteOrderAsync(long orderId);
    }
}
