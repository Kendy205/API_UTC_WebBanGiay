using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IMyOrdersService
    {
        Task<ApiResponse<IEnumerable<OrderDto>>> GetMyOrdersAsync(long currentUserId);
        Task<ApiResponse<OrderDto?>> GetMyOrderByIdAsync(long orderId, long currentUserId);
        Task<ApiResponse<OrderDto>> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId);
        Task<ApiResponse<bool>> CancelMyOrderAsync(long orderId, long currentUserId);
    }
}
