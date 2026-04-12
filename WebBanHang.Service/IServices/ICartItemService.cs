using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface ICartItemService
    {
        Task<IEnumerable<CartItemDto>> GetCartItemsByCartIdAsync(long cartId);
        Task AddProductToCartAsync(long cartId, long variantId, int quantity);
        Task<CartDto> UpdateQuantityAsync(long cartItemId, int newQuantity);
        Task<CartDto> RemoveFromCartAsync(long cartItemId);
        Task<bool> ClearCartAsync(long cartId);
    }
}
