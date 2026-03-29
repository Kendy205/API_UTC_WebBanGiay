using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRoleDto>> GetAllAsync();
        Task<UserRoleDto?> GetByIdAsync(long id);
        Task AddAsync(UserRoleDto dto);
        Task UpdateAsync(long id, UserRoleDto dto);
        Task DeleteAsync(long id);
    }
}
