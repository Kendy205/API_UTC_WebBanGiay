using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<ReviewDto?> GetByIdAsync(long id);
        Task AddAsync(ReviewDto dto);
        Task UpdateAsync(long id, ReviewDto dto);
        Task DeleteAsync(long id);

        // Admin listing with paging and optional rating filter.
        // Returns tuple: items and total count (before paging).
        Task<(IEnumerable<AdminReviewItemDto> Items, int Total)> GetAdminReviewsAsync(int page, int pageSize, int? rating);

        Task<bool> SetVisibilityAsync(long id, bool isVisible);
    }
}
