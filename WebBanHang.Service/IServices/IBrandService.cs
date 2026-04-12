using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(long id);
        Task AddAsync(BrandDto dto);
        Task UpdateAsync(long id, BrandDto dto);
        Task DeleteAsync(long id);
    }
}
