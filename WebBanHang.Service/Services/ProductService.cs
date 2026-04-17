using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(int? pageSize, int? page)
        {
            var entities = await _unitOfWork.Product.GetAllAsync(null, "Category,Brand", pageSize, page);
            entities = entities.Where(x => x.IsActive).ToList();
            int totalCount = await _unitOfWork.Product.CountAsync(x => x.IsActive);
            return new PagedResult<ProductDto>
            {
                Data = _mapper.Map<IEnumerable<ProductDto>>(entities),
                Total = totalCount,
                Page = page ?? 1,
                PageSize = pageSize ?? totalCount
            };
            //return _mapper.Map<IEnsumerable<ProductDto>>(new PagedResult<>);
        }

        public async Task<ProductDto?> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id, "Category,Brand");
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto> AddAsync(ProductDto dto, IFormFile file)
        {
            var entity = _mapper.Map<Product>(dto);

            if (file != null)
            {
                var result = await _photoService.AddPhotoAsync(file);
                if (result == null)
                {
                    throw new Exception("Lỗi khi tải ảnh lên Cloudinary");
                }

                entity.Image = result.SecureUrl.ToString();
                entity.ImagePublicId = result.PublicId;
            }

            await _unitOfWork.Product.AddAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task UpdateAsync(long id, ProductDto dto)
        {
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
            var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id);
            if (entity != null)
            {
                _unitOfWork.Product.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<PagedResult<ProductDto>> GetFilteredProductsAsync(string? keyword, long? categoryId, long? brandId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            Expression<Func<Product, bool>> filter = x => x.IsActive &&
        (string.IsNullOrEmpty(keyword) || x.ProductName.Contains(keyword)) &&
        (!categoryId.HasValue || x.CategoryId == categoryId) &&
        (!brandId.HasValue || x.BrandId == brandId) &&
        (!minPrice.HasValue || x.BasePrice >= minPrice) &&
        (!maxPrice.HasValue || x.BasePrice <= maxPrice);

            int totalCount = await _unitOfWork.Product.CountAsync(filter);

            var entities = await _unitOfWork.Product.GetAllAsync(
                filter: filter,
                includeProperties: "Category,Brand",
                pageSize: pageSize,
                pageNumber: pageNumber,
                orderBy: q => q.OrderByDescending(p => p.ProductId) // Đảm bảo hàng mới hiện lên đầu

            );
            return new PagedResult<ProductDto>
            {
                Data = _mapper.Map<IEnumerable<ProductDto>>(entities), // 8 sản phẩm
                Total = totalCount,   // Tổng số ví dụ: 10
                Page = pageNumber,    // Trang hiện tại
                PageSize = pageSize   // 8
            };
        }
    }
}
