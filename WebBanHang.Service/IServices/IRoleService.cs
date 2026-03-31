using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(long id);
        Task AddAsync(RoleDto dto);
        Task UpdateAsync(long id, RoleDto dto);
        Task DeleteAsync(long id);
    }
}
