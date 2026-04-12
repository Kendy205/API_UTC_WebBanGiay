using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IOrderService
    {
        Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersAsync(long userId, bool isAdmin);
        Task<ApiResponse<OrderDto?>> GetByIdAsync(long id, long userId, bool isAdmin);
        Task<ApiResponse<AdminOrderListResponseDto>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto);
        Task<ApiResponse<AdminOrderDetailDto?>> GetAdminOrderDetailAsync(long id);
        Task<ApiResponse<AdminOrderStatusResultDto>> AdminUpdateOrderStatusAsync(long id, string status, long adminUserId);
        Task<ApiResponse<OrderDto>> PlaceOrderAsync(CheckoutDto checkoutDto, long userId);
        Task<ApiResponse<OrderDto>> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto);
        Task<ApiResponse<bool>> CancelOrderAsync(long orderId, long? currentUserId = null);
        Task<ApiResponse<bool>> DeleteAsync(long id, long deletedByUserId);
    }
}
