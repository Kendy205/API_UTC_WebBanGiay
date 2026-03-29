using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Order.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(entities);
        }

        public async Task<OrderDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.OrderId == id) vào nhé.
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id);
            return _mapper.Map<OrderDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(OrderDto dto)
        {
            var entity = _mapper.Map<Order>(dto);
            await _unitOfWork.Order.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, OrderDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Order.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id);
            if (entity != null)
            {
                _unitOfWork.Order.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
