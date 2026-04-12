using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Address.GetAllAsync();
            return _mapper.Map<IEnumerable<AddressDto>>(entities);
        }

        public async Task<AddressDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.AddressId == id) vào nhé.
            var entity = await _unitOfWork.Address.GetFirstOrDefaultAsync(x => x.AddressId == id);
            return _mapper.Map<AddressDto>(entity);
        }

        public async Task AddAsync(AddressDto dto)
        {
            var entity = _mapper.Map<Address>(dto);
            await _unitOfWork.Address.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, AddressDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Address.GetFirstOrDefaultAsync(x => x.AddressId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Address.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Address.GetFirstOrDefaultAsync(x => x.AddressId == id);
            if (entity != null)
            {
                _unitOfWork.Address.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(long userId)
        {
            var entity = await _unitOfWork.Address.GetAllAsync(x => x.UserId == userId);
            return _mapper.Map<IEnumerable<AddressDto>>(entity);
        }
    }
}
