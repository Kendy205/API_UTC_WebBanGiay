using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Role.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(entities);
        }

        public async Task<RoleDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.RoleId == id) vào nhé.
            var entity = await _unitOfWork.Role.GetFirstOrDefaultAsync(x => x.RoleId == id);
            return _mapper.Map<RoleDto?>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(RoleDto dto)
        {
            var entity = _mapper.Map<Role>(dto);
            await _unitOfWork.Role.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, RoleDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Role.GetFirstOrDefaultAsync(x => x.RoleId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Role.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
             var entity = await _unitOfWork.Role.GetFirstOrDefaultAsync(x => x.RoleId == id);
            if (entity != null)
            {
                _unitOfWork.Role.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
