using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartItemService
    {
        /// <summary>Lấy danh sách item theo cartId (không kèm cart)</summary>
        Task<IEnumerable<CartItemDto>> GetCartItemsByCartIdAsync(long cartId);

        /// <summary>Thêm sản phẩm vào giỏ</summary>
        Task AddProductToCartAsync(long cartId, long variantId, int quantity);

        /// <summary>Cập nhật số lượng item, trả về toàn bộ cart sau khi cập nhật</summary>
        Task<CartDto> UpdateQuantityAsync(long cartItemId, int newQuantity);

        /// <summary>Xóa một item khỏi giỏ, trả về toàn bộ cart sau khi xóa</summary>
        Task<CartDto> RemoveFromCartAsync(long cartItemId);

        /// <summary>Xóa toàn bộ item trong giỏ</summary>
        Task<bool> ClearCartAsync(long cartId);
    }
}