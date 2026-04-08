using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface ICartService
    {
        /// <summary>Lấy giỏ hàng theo ID (kèm danh sách sản phẩm)</summary>
        Task<CartDto?> GetByIdAsync(long id);
        Task AddAsync(CartDto dto);
        Task UpdateAsync(long id, CartDto dto);
        Task DeleteAsync(long id);

        Task<CartDto> GetCartByUserId(long userId); 


        /// <summary>Lấy hoặc tạo giỏ hàng active của user</summary>
        Task<CartDto> GetOrCreateCartForUserAsync(long userId);

        /// <summary>Lấy giỏ hàng active của user (nếu có)</summary>
        Task<CartDto?> GetActiveCartByUserIdAsync(long userId);

        /// <summary>Cập nhật trạng thái giỏ hàng</summary>
        Task<bool> UpdateStatusAsync(long cartId, string newStatus);

    }
}