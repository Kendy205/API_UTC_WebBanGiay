using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductVariantService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductVariantDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.ProductVariant.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductVariantDto>>(entities);
        }

        public async Task<ProductVariantDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.ProductVariantId == id) vào nhé.
            var entity = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == id);
            return _mapper.Map<ProductVariantDto>(entity)  ; // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(ProductVariantDto dto)
        {
            var entity = _mapper.Map<ProductVariant>(dto);
            await _unitOfWork.ProductVariant.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, ProductVariantDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.ProductVariant.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == id);
            if (entity != null)
            {
                _unitOfWork.ProductVariant.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
