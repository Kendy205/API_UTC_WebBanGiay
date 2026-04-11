using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.DTOs.Common;

namespace WebBanHang.Service.IServices
{
    public interface IOrderService
    {
        Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersAsync(long userId, bool isAdmin);
        Task<ApiResponse<OrderDto?>> GetByIdAsync(long id, long userId, bool isAdmin);
        Task<ApiResponse<OrderDto>> PlaceOrderAsync(CheckoutDto checkoutDto, long userId);
        Task<ApiResponse<OrderDto>> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto);
        Task<ApiResponse<bool>> CancelOrderAsync(long orderId, long? currentUserId = null);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
