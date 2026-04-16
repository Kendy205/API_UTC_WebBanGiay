using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Category.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(entities);
        }

        public async Task<CategoryDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.CategoryId == id) vào nhé.
            var entity = await _unitOfWork.Category.GetFirstOrDefaultAsync(x => x.CategoryId == id);
            return _mapper.Map<CategoryDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(CategoryDto dto)
        {
            var entity = _mapper.Map<Category>(dto);
            entity.ParentId = null; // TODO: Nếu có hỗ trợ cây danh mục, bạn cần xử lý ParentId ở đây
            await _unitOfWork.Category.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, CategoryDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Category.GetFirstOrDefaultAsync(x => x.CategoryId == id);
            entity.ParentId = null;
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Category.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Category.GetFirstOrDefaultAsync(x => x.CategoryId == id);
            entity.ParentId = null;
            if (entity != null)
            {
                _unitOfWork.Category.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
