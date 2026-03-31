using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.DTOs.Common;

namespace WebBanHang.Service.IServices
{
    public interface IOrderService
    {
        Task<ApiResponse<IEnumerable<OrderDto>>> GetAllAsync();
        Task<ApiResponse<OrderDto?>> GetByIdAsync(long id);
        Task<ApiResponse<IEnumerable<OrderDto>>> GetByUserIdAsync(long userId);
        Task<ApiResponse<OrderDto>> PlaceOrderAsync(CheckoutDto checkoutDto);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(long orderId, string status);
        Task<ApiResponse<bool>> UpdatePaymentStatusAsync(long orderId, string status);
        Task<ApiResponse<bool>> CancelOrderAsync(long orderId);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
