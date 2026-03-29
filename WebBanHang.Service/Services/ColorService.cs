using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class ColorService : IColorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ColorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ColorDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Color.GetAllAsync();
            return _mapper.Map<IEnumerable<ColorDto>>(entities);
        }

        public async Task<ColorDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.ColorId == id) vào nhé.
            var entity = await _unitOfWork.Color.GetFirstOrDefaultAsync(x => x.ColorId == id);
            return _mapper.Map<ColorDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(ColorDto dto)
        {
            var entity = _mapper.Map<Color>(dto);
            await _unitOfWork.Color.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, ColorDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Color.GetFirstOrDefaultAsync(x => x.ColorId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Color.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Color.GetFirstOrDefaultAsync(x => x.ColorId == id);
            if (entity != null)
            {
                _unitOfWork.Color.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
