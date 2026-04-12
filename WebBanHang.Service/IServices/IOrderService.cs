using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync(long userId, bool isAdmin);
        Task<OrderDto> GetByIdAsync(long id, long userId, bool isAdmin);
        Task<AdminOrderListResponseDto> GetAdminOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderDetailDto> GetAdminOrderDetailAsync(long id);
        Task<AdminOrderStatusResultDto> AdminUpdateOrderStatusAsync(long id, string status, long adminUserId);
        Task<OrderDto> PlaceOrderAsync(CheckoutDto checkoutDto, long userId);
        Task<OrderDto> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto);
        Task CancelOrderAsync(long orderId, long? currentUserId = null);
        Task DeleteAsync(long id, long deletedByUserId);
    }
}
