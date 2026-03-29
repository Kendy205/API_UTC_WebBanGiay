using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartItemService
    {
        Task<IEnumerable<CartItemDto>> GetAllAsync();
        Task<CartItemDto?> GetByIdAsync(long id);
        Task AddAsync(CartItemDto dto);
        Task UpdateAsync(long id, CartItemDto dto);
        Task DeleteAsync(long id);
    }
}
