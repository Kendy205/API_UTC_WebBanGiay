using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<ReviewDto?> GetByIdAsync(long id);
        Task AddAsync(ReviewDto dto);
        Task UpdateAsync(long id, ReviewDto dto);
        Task DeleteAsync(long id);
    }
}
