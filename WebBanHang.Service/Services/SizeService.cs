using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class SizeService : ISizeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SizeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SizeDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Size.GetAllAsync();
            return _mapper.Map<IEnumerable<SizeDto>>(entities);
        }

        public async Task<SizeDto?> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Size.GetFirstOrDefaultAsync(x => x.SizeId == id);
            return _mapper.Map<SizeDto?>(entity);
        }

        public async Task AddAsync(SizeDto dto)
        {
            var entity = _mapper.Map<Size>(dto);
            await _unitOfWork.Size.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, SizeDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Size.GetFirstOrDefaultAsync(x => x.SizeId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Size.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Size.GetFirstOrDefaultAsync(x => x.SizeId == id);
            if (entity != null)
            {
                _unitOfWork.Size.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
