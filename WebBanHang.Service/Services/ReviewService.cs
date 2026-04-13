using AutoMapper;
using System.Collections.Generic;
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

        public async Task<IEnumerable<ReviewDto>> GetByProductIdAsync(long productId)
        {
            var entities = await _unitOfWork.Review.GetAllAsync(x => x.OrderItem.ProductVariant.ProductId == productId, "OrderItem,OrderItem.ProductVariant");
            return _mapper.Map<IEnumerable<ReviewDto>>(entities);
        }
    }
}
