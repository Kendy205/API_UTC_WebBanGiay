using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Cart.GetAllAsync();
            return _mapper.Map<IEnumerable<CartDto>>(entities);
        }

        public async Task<CartDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.CartId == id) vào nhé.
           var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.CartId == id);
            return _mapper.Map<CartDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(CartDto dto)
        {
            var entity = _mapper.Map<Cart>(dto);
            await _unitOfWork.Cart.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, CartDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.CartId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Cart.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.CartId == id);
            if (entity != null)
            {
                _unitOfWork.Cart.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
