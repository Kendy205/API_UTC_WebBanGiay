using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryMovementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.InventoryMovement.GetAllAsync();
            return _mapper.Map<IEnumerable<InventoryMovementDto>>(entities);
        }

        public async Task<InventoryMovementDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.InventoryMovementId == id) vào nhé.
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            return _mapper.Map<InventoryMovementDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(InventoryMovementDto dto)
        {
            var entity = _mapper.Map<InventoryMovement>(dto);
            await _unitOfWork.InventoryMovement.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, InventoryMovementDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.InventoryMovement.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            if (entity != null)
            {
                _unitOfWork.InventoryMovement.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
