using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartService
    {
        /// <summary>Lấy giỏ hàng theo ID (kèm danh sách sản phẩm)</summary>
        Task<CartDto?> GetByIdAsync(long id);

        Task UpdateAsync(long id, UpdateToCartRequest dto);

        /// <summary>Lấy hoặc tạo giỏ hàng active của user</summary>
        Task<CartDto> GetOrCreateCartForUserAsync(long userId);

        /// <summary>Lấy giỏ hàng active của user (nếu có)</summary>
        Task<CartDto?> GetActiveCartByUserIdAsync(long userId);
    }
}