using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IAdminOrdersService
    {
        Task<AdminOrderListResponseDto> GetOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderDetailDto?> GetOrderDetailAsync(long id);
        Task<AdminOrderStatusResultDto> UpdateOrderStatusAsync(long id, string status);
        Task<OrderDto> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto);
        Task<bool> DeleteOrderAsync(long orderId);
        Task<OrderDto> CreateOrderAsync(CheckoutDto checkoutDto, long customerId);
    }
}
