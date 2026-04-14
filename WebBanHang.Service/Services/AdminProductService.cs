using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;


        public AdminProductService(IUnitOfWork unitOfWork, IPhotoService photoService , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductAdminDto>> GetProductsAsync(string? search, int page, int pageSize)
        {
            Expression<Func<Product, bool>> filter = x =>
                string.IsNullOrEmpty(search) || x.ProductName.Contains(search);

            var entities = await _unitOfWork.Product.GetAllAsync(
                filter: filter,
                includeProperties: "Category,Brand,ProductVariants,ProductVariants.Color,ProductVariants.Size",
                pageSize: pageSize,
                pageNumber: page
            );

            var allItems = await _unitOfWork.Product.GetAllAsync(filter);
            int totalCount = allItems.Count();
            // 🔥 lấy inventory movements
            var movements = await _unitOfWork.InventoryMovement.GetAllAsync(
                includeProperties: "ProductVariant"
            );

            // 🔥 group sold theo product
            var soldDict = movements
                .Where(m => m.MovementType == "OUT" && m.ReferenceType == "order")
                .Where(m => m.ProductVariant != null)
                .GroupBy(m => m.ProductVariant.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Quantity)
                );

            var data = entities.Select(x => new ProductAdminDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                BasePrice = x.BasePrice,

                CategoryId = x.CategoryId,
                BrandId = x.BrandId,
                CategoryName = x.Category?.CategoryName,
                BrandName = x.Brand.BrandName,
                ImageUrl = x.Image ?? "no-image.jpg",
                IsActive = x.IsActive,
                Sold = soldDict.ContainsKey(x.ProductId) ? soldDict[x.ProductId] : 0,
                ProductVariants = x.ProductVariants.Select(pv => new ProductVariantDto
                {
                    VariantId = pv.VariantId,
                    ColorName = pv.Color.ColorName,
                    SizeLabel = pv.Size.SizeLabel,
                    ColorId = pv.ColorId,
                    SizeId = pv.SizeId,
                    PriceOverride = pv.PriceOverride,
                    StockQuantity = pv.StockQuantity,
                    IsActive = pv.IsActive,
                    ProductId = pv.ProductId,
                    Sku = pv.Sku
                    
                }).ToList()

            });

            return new PagedResult<ProductAdminDto>
            {
                Data = data,
                Total = totalCount,
                Page = page,
                PageSize = pageSize
    };
}

        //public async Task<ProductAdminDto> CreateAsync(ProductAdminDto dto, IFormFile file)
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

        public async Task UpdateAsync(long id, ProductAdminDto dto)
{
    var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id);

    if (entity == null)
        throw new Exception("Không tìm thấy sản phẩm");

    entity.ProductName = dto.ProductName;
    entity.BasePrice = dto.BasePrice;

    entity.CategoryId = dto.CategoryId;
    entity.IsActive = dto.IsActive;

    _unitOfWork.Product.Update(entity);
    await _unitOfWork.SaveAsync();
}

public async Task DeleteAsync(long id)
{
    var entity = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.ProductId == id);

    if (entity == null)
        throw new Exception("Không tìm thấy sản phẩm");

    _unitOfWork.Product.Remove(entity);
    await _unitOfWork.SaveAsync();
}
    }
}
