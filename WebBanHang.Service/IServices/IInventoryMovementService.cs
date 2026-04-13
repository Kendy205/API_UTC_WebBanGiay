using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

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
    }
}
