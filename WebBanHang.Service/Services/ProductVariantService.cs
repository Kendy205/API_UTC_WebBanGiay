using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
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
            var entities = await _unitOfWork.ProductVariant.GetAllAsync(null, "Product,Size,Color");
            return _mapper.Map<IEnumerable<ProductVariantDto>>(entities);
        }

        public async Task<ProductVariantDto?> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == id, "Size,Color,Product");
            return _mapper.Map<ProductVariantDto>(entity);
        }

        public async Task<IEnumerable<ProductVariantDto>> GetByProductIdAsync(long productId)
        {
            var entities = await _unitOfWork.ProductVariant.GetAllAsync(x => x.ProductId == productId, "Color,Size,Product");
            return _mapper.Map<IEnumerable<ProductVariantDto>>(entities);
        }

        public async Task AddAsync(ProductVariantDto dto)
        {
            var entity = _mapper.Map<ProductVariant>(dto);
            await _unitOfWork.ProductVariant.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, ProductVariantDto dto)
        {
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
            var entity = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == id);
            if (entity != null)
            {
                _unitOfWork.ProductVariant.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
