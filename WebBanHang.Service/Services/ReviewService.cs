using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

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
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            return _mapper.Map<ReviewDto>(entity);
        }

        public async Task AddAsync(ReviewDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.UserId <= 0) throw new InvalidOperationException("UserId is required to create a review.");

            // 1. Load order item and its order to validate purchase & payment status
            var orderItem = await _unitOfWork.OrderItem.GetFirstOrDefaultAsync(
                oi => oi.OrderItemId == dto.OrderItemId,
                includeProperties: "Order"
            );

            if (orderItem == null)
                throw new InvalidOperationException("Order item not found.");

            if (orderItem.Order == null)
                throw new InvalidOperationException("Related order not found for the order item.");

            // 2. Ensure the order has been paid
            //    PaymentStatus values: e.g. "unpaid" | "paid" | "failed" | "refunded"
            if (!string.Equals(orderItem.Order.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Order must be paid before leaving a review.");

            // 3. Ensure the user who posts the review is the purchaser of this order item
            if (orderItem.Order.UserId != dto.UserId)
                throw new InvalidOperationException("User is not the purchaser of this order item.");

            // 4. Ensure the user hasn't already reviewed this order item (one review per order-item per user)
            var existing = await _unitOfWork.Review.GetFirstOrDefaultAsync(
                r => r.OrderItemId == dto.OrderItemId && r.UserId == dto.UserId
            );
            if (existing != null)
                throw new InvalidOperationException("You have already reviewed this item.");

            // 5. Map and persist
            var entity = _mapper.Map<Review>(dto);
            // ensure sensible defaults if not set
            entity.CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt;
            await _unitOfWork.Review.AddAsync(entity);

            await _unitOfWork.SaveAsync();

            // Optionally update dto.ReviewId so callers that rely on it can use it.
            dto.ReviewId = entity.ReviewId;
        }

        public async Task UpdateAsync(long id, ReviewDto dto)
        {
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
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            if (entity != null)
            {
                _unitOfWork.Review.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<(IEnumerable<AdminReviewItemDto> Items, int Total)> GetAdminReviewsAsync(int page, int pageSize, int? rating)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            System.Linq.Expressions.Expression<Func<Review, bool>>? filter = null;
            if (rating.HasValue)
            {
                filter = r => r.Rating == rating.Value;
            }

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
            var entity = await _unitOfWork.Review.GetFirstOrDefaultAsync(x => x.ReviewId == id);
            if (entity == null) return false;
            entity.IsPublic = isVisible;
            _unitOfWork.Review.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
