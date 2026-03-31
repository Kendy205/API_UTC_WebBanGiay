using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetAllAsync();
        Task<CartDto?> GetByIdAsync(long id);
        Task AddAsync(CartDto dto);
        Task UpdateAsync(long id, CartDto dto);
        Task DeleteAsync(long id);
    }
}
