using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.CartItem.GetAllAsync();
            return _mapper.Map<IEnumerable<CartItemDto>>(entities);
        }

        public async Task<CartItemDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.CartItemId == id) vào nhé.
           var entity = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(x => x.CartItemId == id);
            return _mapper.Map<CartItemDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(CartItemDto dto)
        {
            var entity = _mapper.Map<CartItem>(dto);
            await _unitOfWork.CartItem.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, CartItemDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(x => x.CartItemId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.CartItem.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(x => x.CartItemId == id);
            if (entity != null)
            {
                _unitOfWork.CartItem.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
