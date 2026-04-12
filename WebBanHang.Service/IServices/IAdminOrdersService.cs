using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IAdminOrdersService
    {
        Task<ApiResponse<AdminOrderListResponseDto>> GetOrdersAsync(AdminOrderQueryDto queryDto);
        Task<ApiResponse<AdminOrderDetailDto?>> GetOrderDetailAsync(long id);
        Task<ApiResponse<AdminOrderStatusResultDto>> UpdateOrderStatusAsync(long id, string status);
        Task<ApiResponse<OrderDto>> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto);
        Task<ApiResponse<bool>> DeleteOrderAsync(long orderId);
        Task<ApiResponse<OrderDto>> CreateOrderAsync(CheckoutDto checkoutDto, long customerId);
    }
}
