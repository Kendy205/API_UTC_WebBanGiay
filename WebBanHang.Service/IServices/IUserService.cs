using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(long id);
        Task<string> AddAsync(UserResgiterDto dto);
        Task UpdateAsync(long id, UserResgiterDto dto);
        Task DeleteAsync(long id);
    }
}
