using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface ISizeService
    {
        Task<IEnumerable<SizeDto>> GetAllAsync();
        Task<SizeDto?> GetByIdAsync(long id);
        Task AddAsync(SizeDto dto);
        Task UpdateAsync(long id, SizeDto dto);
        Task DeleteAsync(long id);
    }
}
