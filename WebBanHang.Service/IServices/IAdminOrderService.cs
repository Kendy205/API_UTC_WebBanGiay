using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IAdminOrderService
    {
        Task<ApiResponse<AdminOrderListResponseDto>> GetOrdersAsync(AdminOrderQueryDto queryDto);
        Task<ApiResponse<AdminOrderDetailDto?>> GetOrderByIdAsync(long orderId);
        Task<ApiResponse<AdminOrderStatusResultDto>> UpdateOrderStatusAsync(long orderId, string status, long adminUserId);
        Task<ApiResponse<OrderDto>> CheckoutAsync(CheckoutDto checkoutDto, long adminUserId);
        Task<ApiResponse<OrderDto>> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto);
        Task<ApiResponse<bool>> DeleteOrderAsync(long orderId, long adminUserId);
    }
}
