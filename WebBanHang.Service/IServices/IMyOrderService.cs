using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IMyOrderService
    {
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long currentUserId);
        Task<OrderDto> GetMyOrderByIdAsync(long orderId, long currentUserId);
        Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId);
        Task CancelMyOrderAsync(long orderId, long currentUserId);
    }
}
