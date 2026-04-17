using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;

namespace WebBanHang.Service.IServices
{
    public interface IInventoryMovementService
    {
        Task<IEnumerable<InventoryMovementDto>> GetAllAsync();
        Task<InventoryMovementDto?> GetByIdAsync(long id);
        Task AddAsync(InventoryMovementDto dto);
        Task AddRangeAsync(IEnumerable<InventoryMovementDto> dtos);
        Task UpdateAsync(long id, InventoryMovementDto dto);
        Task DeleteAsync(long id);

        // Các phương thức điều phối nghiệp vụ kho
        Task HandleCheckoutAsync(Order order, IEnumerable<CartItemLocalDto> items, long userId);
        Task HandleOrderCancelAsync(Order order, long userId);
    }
}