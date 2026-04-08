using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IColorService
    {
        Task<IEnumerable<ColorDto>> GetAllAsync();
        Task<ColorDto?> GetByIdAsync(long id);
        Task AddAsync(ColorDto dto);
        Task UpdateAsync(long id, ColorDto dto);
        Task DeleteAsync(long id);
    }
}
