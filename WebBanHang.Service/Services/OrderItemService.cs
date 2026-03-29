using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderItemDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.OrderItem.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderItemDto>>(entities);
        }

        public async Task<OrderItemDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.OrderItemId == id) vào nhé.
            var entity = await _unitOfWork.OrderItem.GetFirstOrDefaultAsync(x => x.OrderItemId == id);
            return _mapper.Map<OrderItemDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(OrderItemDto dto)
        {
            var entity = _mapper.Map<OrderItem>(dto);
            await _unitOfWork.OrderItem.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, OrderItemDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.OrderItem.GetFirstOrDefaultAsync(x => x.OrderItemId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.OrderItem.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.OrderItem.GetFirstOrDefaultAsync(x => x.OrderItemId == id);
            if (entity != null)
            {
                _unitOfWork.OrderItem.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
