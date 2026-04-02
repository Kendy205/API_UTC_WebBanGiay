using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(long id);
        Task<OrderDto?> CreateOrderFromCart(long cartId);
        Task AddAsync(OrderDto dto);
        Task UpdateAsync(long id, OrderDto dto);
        Task DeleteAsync(long id);
    }
}
