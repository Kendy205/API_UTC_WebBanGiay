using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Payment.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(entities);
        }

        public async Task<PaymentDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.PaymentId == id) vào nhé.
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            return _mapper.Map<PaymentDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(PaymentDto dto)
        {
            var entity = _mapper.Map<Payment>(dto);
            await _unitOfWork.Payment.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, PaymentDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Payment.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            if (entity != null)
            {
                _unitOfWork.Payment.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
