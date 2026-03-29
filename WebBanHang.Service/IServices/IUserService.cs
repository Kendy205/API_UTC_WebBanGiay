using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(long id);
        Task AddAsync(UserDto dto);
        Task UpdateAsync(long id, UserDto dto);
        Task DeleteAsync(long id);
    }
}
