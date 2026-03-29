using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IInventoryMovementService
    {
        Task<IEnumerable<InventoryMovementDto>> GetAllAsync();
        Task<InventoryMovementDto?> GetByIdAsync(long id);
        Task AddAsync(InventoryMovementDto dto);
        Task UpdateAsync(long id, InventoryMovementDto dto);
        Task DeleteAsync(long id);
    }
}
