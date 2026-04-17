using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IMyOrdersService
    {
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long currentUserId, int pageNumber, int pageSize);
        Task<OrderDto?> GetMyOrderByIdAsync(long orderId, long currentUserId);
        Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId);
        Task<bool> CancelMyOrderAsync(long orderId, long currentUserId);
    }
}
