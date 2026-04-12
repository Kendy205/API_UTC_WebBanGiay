using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.User.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(entities);
        }

        public async Task<UserDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.UserId == id) vào nhé.
            var entity = await _unitOfWork.User.GetFirstOrDefaultAsync(x => x.UserId == id);
            return _mapper.Map<UserDto?>(entity);
        }

        public async Task AddAsync(UserDto dto)
        {
            var entity = _mapper.Map<User>(dto);
            await _unitOfWork.User.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, UserDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.User.GetFirstOrDefaultAsync(x => x.UserId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.User.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.User.GetFirstOrDefaultAsync(x => x.UserId == id);
            if (entity != null)
            {
                _unitOfWork.User.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
