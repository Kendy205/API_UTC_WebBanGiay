using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using System.Reflection.Metadata.Ecma335;

namespace WebBanHang.Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Review.GetAllAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(entities);
        }

        public async Task<ReviewDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.ReviewId == id) vào nhé.
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            return _mapper.Map<ReviewDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(ReviewDto dto)
        {
            var entity = _mapper.Map<Review>(dto);
            await _unitOfWork.Review.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, ReviewDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Review.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            if (entity != null)
            {
                _unitOfWork.Review.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        // New method to support admin UI: paging + optional rating filter.
        public async Task<(IEnumerable<AdminReviewItemDto> Items, int Total)> GetAdminReviewsAsync(int page, int pageSize, int? rating)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // Ensure related navigation properties are loaded from DB (OrderItem, User).
            System.Linq.Expressions.Expression<Func<Review, bool>>? filter = null;
            if (rating.HasValue)
            {
                filter = r => r.Rating == rating.Value;
            }

            // Load matching reviews including navigation properties
            var allMatching = await _unitOfWork.Review.GetAllAsync(filter, "OrderItem,User");

            var total = allMatching.Count();

            var items = allMatching
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminReviewItemDto
                {
                    Id = r.ReviewId,
                    ProductName = r.OrderItem != null ? r.OrderItem.ProductNameSnapshot : string.Empty,
                    CustomerName = r.User != null ? r.User.FullName : string.Empty,
                    Rating = r.Rating,
                    Comment = r.ReviewContent,
                    CreatedAt = r.CreatedAt,
                    IsVisible = r.IsPublic
                })
                .ToList();

            return (items, total);
        }

        public async Task<bool> SetVisibilityAsync(long id, bool isVisible)
        {
            var entity = _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id).Result;
            if (entity == null) return false;
            entity.IsPublic = isVisible;
            _unitOfWork.Review.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
