using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartService
    {
        /// <summary>Lấy giỏ hàng theo ID (kèm danh sách sản phẩm)</summary>
        Task<CartDto?> GetByIdAsync(long id);
        Task AddAsync(CartDto dto);
        Task UpdateAsync(long id, CartDto dto);
        Task DeleteAsync(long id);

        Task<CartDto> GetCartByUserId(long userId); 

        /// <summary>Cập nhật trạng thái giỏ hàng</summary>
        Task<bool> UpdateStatusAsync(long cartId, string newStatus);

    }
}