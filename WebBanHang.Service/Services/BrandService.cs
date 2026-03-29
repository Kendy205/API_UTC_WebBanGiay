using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BrandService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Brand.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandDto>>(entities);
        }

        public async Task<BrandDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.BrandId == id) vào nhé.
            // Ví dụ chuẩn: var entity = await _unitOfWork.Brand.GetFirstOrDefaultAsync(x => x.BrandId == id);
            var entity = await _unitOfWork.Brand.GetFirstOrDefaultAsync(x => x.BrandId == id);
            return _mapper.Map<BrandDto>(entity);
        }

        public async Task AddAsync(BrandDto dto)
        {
            var entity = _mapper.Map<Brand>(dto);
            await _unitOfWork.Brand.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, BrandDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Brand.GetFirstOrDefaultAsync(x => x.BrandId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Brand.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Brand.GetFirstOrDefaultAsync(x => x.BrandId == id);
            if (entity != null)
            {
                _unitOfWork.Brand.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
