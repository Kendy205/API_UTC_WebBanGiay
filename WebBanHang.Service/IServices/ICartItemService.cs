using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface ICartItemService
    {
        /// <summary>Xóa toàn bộ item trong giỏ</summary>
        Task<bool> ClearCartAsync(long cartId);
    }
}