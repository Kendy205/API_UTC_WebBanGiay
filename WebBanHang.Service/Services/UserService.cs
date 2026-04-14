using AutoMapper;
using CloudinaryDotNet.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Auth;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

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
            var entities = await _unitOfWork.User.GetAllAsync(null, "UserRoles,UserRoles.Role");
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

        public async Task UpdateAsync(long id, UserResgiterDto dto)
        {
            var entity = await _unitOfWork.User.GetFirstOrDefaultAsync(
            x => x.UserId == id,
            includeProperties: "UserRoles"
            );

            if (entity == null) throw new Exception("User không tồn tại");

            _mapper.Map(dto, entity);
            // ... logic hash password ...
            if (!string.IsNullOrEmpty(dto.Password))
            {
                entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            if (dto.RoleId.HasValue)
            {
                var currentUserRole = entity.UserRoles.FirstOrDefault();

                // Kiểm tra xem RoleId mới có khác RoleId cũ không
                if (currentUserRole == null || currentUserRole.RoleId != dto.RoleId.Value)
                {
                    // 1. Nếu đã có Role cũ, phải xóa nó đi trước
                    if (currentUserRole != null)
                    {
                        // Xóa bản ghi cũ khỏi context
                        _unitOfWork.UserRole.Remove(currentUserRole);

                        // CỰC KỲ QUAN TRỌNG: Phải SaveChanges một lần ở đây để DB xóa xong khóa cũ
                        await _unitOfWork.SaveAsync();
                    }

                    // 2. Thêm bản ghi Role mới với khóa mới
                    var newUserRole = new UserRole
                    {
                        UserId = id,
                        RoleId = dto.RoleId.Value
                    };
                    await _unitOfWork.UserRole.AddAsync(newUserRole);
                }
            }

            _unitOfWork.User.Update(entity);
            await _unitOfWork.SaveAsync();
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

        public async Task<string> AddAsync(UserResgiterDto dto)
        {
            var existingUser = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return "Email đã được sử dụng!";
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var newUser = new User
            {
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = passwordHash
            };

            var newCart = new Cart
            {
                User = newUser
            };

            await _unitOfWork.User.AddAsync(newUser);
            await _unitOfWork.Cart.AddAsync(newCart);
            await _unitOfWork.SaveAsync();
            short roleId = dto.RoleId ?? 2; // Mặc định là role "User" nếu không có RoleId nào được cung cấp
            var userRole = new UserRole { UserId = newUser.UserId, RoleId = roleId };
            await _unitOfWork.UserRole.AddAsync(userRole);
            await _unitOfWork.SaveAsync();

            return "Đăng ký thành công!";
        }
    }
}
