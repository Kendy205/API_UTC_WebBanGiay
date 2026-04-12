using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserRoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserRoleDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.UserRole.GetAllAsync();
            return _mapper.Map<IEnumerable<UserRoleDto>>(entities);
        }

        public async Task<UserRoleDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.UserRoleId == id) vào nhé.
            // Ví dụ chuẩn: var entity = await _unitOfWork.UserRole.GetFirstOrDefaultAsync(x => x.UserRoleId == id);
            return null; // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(UserRoleDto dto)
        {
            var entity = _mapper.Map<UserRole>(dto);
            await _unitOfWork.UserRole.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, UserRoleDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            // var entity = await _unitOfWork.UserRole.GetFirstOrDefaultAsync(x => x.UserRoleId == id);
            // if (entity != null) {
            //     _mapper.Map(dto, entity);
            //     _unitOfWork.UserRole.Update(entity);
            //     await _unitOfWork.SaveAsync();
            // }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            // var entity = await _unitOfWork.UserRole.GetFirstOrDefaultAsync(x => x.UserRoleId == id);
            // if (entity != null) {
            //     _unitOfWork.UserRole.Remove(entity);
            //     await _unitOfWork.SaveAsync();
            // }
        }
    }
}
