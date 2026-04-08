using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(long id);
        Task AddAsync(CategoryDto dto);
        Task UpdateAsync(long id, CategoryDto dto);
        Task DeleteAsync(long id);
    }
}
