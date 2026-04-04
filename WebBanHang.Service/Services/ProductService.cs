using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Product.GetAllAsync(null, "Category,Brand");
            return _mapper.Map<IEnumerable<ProductDto>>(entities);
        }

        public async Task<ProductDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.ProductId == id) vào nhé.
            var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id, "Category,Brand");
            return _mapper.Map<ProductDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task<ProductDto> AddAsync(ProductDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            await _unitOfWork.Product.AddAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task UpdateAsync(long id, ProductDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Product.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id);
            if (entity != null)
            {
                _unitOfWork.Product.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
