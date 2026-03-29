using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemDto>> GetAllAsync();
        Task<OrderItemDto?> GetByIdAsync(long id);
        Task AddAsync(OrderItemDto dto);
        Task UpdateAsync(long id, OrderItemDto dto);
        Task DeleteAsync(long id);
    }
}
