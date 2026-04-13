using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface ICartService
    {
        Task<CartDto?> GetByIdAsync(long id);
        Task AddAsync(CartDto dto);
        Task UpdateAsync(long id, CartDto dto);
        Task DeleteAsync(long id);
        Task<CartDto> GetCartByUserId(long userId);
        //Task<CartDto> GetOrCreateCartForUserAsync(long userId);
        //Task<CartDto?> GetActiveCartByUserIdAsync(long userId);
        Task<bool> UpdateStatusAsync(long cartId, string newStatus);
    }
}
