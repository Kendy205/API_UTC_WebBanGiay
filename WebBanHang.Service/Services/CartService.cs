using System;
using System.Threading.Tasks;
using AutoMapper;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
         
        public async Task<CartDto?> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(
                x => x.CartId == id,
                includeProperties: "CartItems,CartItems.ProductVariant,CartItems.ProductVariant.Size,CartItems.ProductVariant.Color,CartItems.ProductVariant.Product"
            );
            return _mapper.Map<CartDto>(entity);
        }

        public async Task<CartDto> GetOrCreateCartForUserAsync(long userId)
        {
            var activeCart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(
                x => x.UserId == userId && x.Status == "active",
                includeProperties: "CartItems,CartItems.ProductVariant,CartItems.ProductVariant.Size,CartItems.ProductVariant.Color,CartItems.ProductVariant.Product"
            );

            if (activeCart != null)
                return _mapper.Map<CartDto>(activeCart);

            var newCart = new Cart
            {
                UserId = userId,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Cart.AddAsync(newCart);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<CartDto>(newCart);
        }

        public async Task<CartDto?> GetActiveCartByUserIdAsync(long userId)
        {
            var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(
                x => x.UserId == userId && x.Status == "active",
                includeProperties: "CartItems,CartItems.ProductVariant,CartItems.ProductVariant.Size,CartItems.ProductVariant.Color,CartItems.ProductVariant.Product"
            );
            return _mapper.Map<CartDto>(entity);
        }

        public async Task<bool> UpdateStatusAsync(long cartId, string newStatus)
        {
            var entity = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.CartId == cartId);
            if (entity == null) return false;

            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Cart.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}