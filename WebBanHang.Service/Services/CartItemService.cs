using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cartService = cartService;
        }
         
        public async Task<IEnumerable<CartItemDto>> GetCartItemsByCartIdAsync(long cartId)
        {
            var entities = await _unitOfWork.CartItem.GetAllAsync(
                x => x.CartId == cartId,
                includeProperties: "ProductVariant,ProductVariant.Size,ProductVariant.Color,ProductVariant.Product"
            );
            return _mapper.Map<IEnumerable<CartItemDto>>(entities);
        }

        public async Task AddProductToCartAsync(long cartId, long variantId, int quantity)
        {
            var v = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == variantId);
            if (quantity > v.StockQuantity)
                throw new InvalidOperationException("vượt quá số lượng tồn kho");
            // Kiểm tra sản phẩm đã tồn tại trong giỏ chưa
            var existingItem = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(
                x => x.CartId == cartId && x.VariantId == variantId
            );

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                _unitOfWork.CartItem.Update(existingItem);
            }
            else
            {
                var variant =await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(
                    x => x.VariantId == variantId,
                    includeProperties :"Product"
                );
                if (variant == null)
                    throw new InvalidOperationException("Không thấy biến thể");

                var newItem = new CartItem
                {
                    CartId = cartId,
                    VariantId = variantId,
                    Quantity = quantity,
                    UnitPrice = (decimal)(variant.Product.SalePrice ?? variant.Product.BasePrice),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartItem.AddAsync(newItem);
            }

            await _unitOfWork.SaveAsync();

        }
        public async Task<CartDto> RemoveFromCartAsync(long cartItemId)
        {
            var entity = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(x => x.CartItemId == cartItemId);
            if (entity == null)
                throw new InvalidOperationException("Không tìm thấy CartItem");

            long cartId = entity.CartId;
            _unitOfWork.CartItem.Remove(entity);
            await _unitOfWork.SaveAsync();

            return await _cartService.GetByIdAsync(cartId)
                ?? throw new InvalidOperationException("Không tìm thấy giỏ hàng sau khi xóa sản phẩm");
        }

        public async Task<bool> ClearCartAsync(long cartId)
        {
            var items = await _unitOfWork.CartItem.GetAllAsync(x => x.CartId == cartId);
            if (!items.Any()) return true;

            foreach (var item in items)
            {
                _unitOfWork.CartItem.Remove(item);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<CartDto> UpdateQuantityAsync(long cartItemId, int newQuantity)
        {
            var cartItem = await _unitOfWork.CartItem.GetFirstOrDefaultAsync(x => x.CartItemId == cartItemId);
            if (cartItem == null)
                throw new InvalidOperationException("Không tìm thấy CartItem");

            var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == cartItem.VariantId);
            if (newQuantity > variant.StockQuantity)
                throw new InvalidOperationException("vượt quá số lượng tồn kho");

            if (newQuantity == 0)
                return await RemoveFromCartAsync(cartItemId);

            long cartId = cartItem.CartId;
            cartItem.Quantity = newQuantity;
            _unitOfWork.CartItem.Update(cartItem);
            await _unitOfWork.SaveAsync();

            return await _cartService.GetByIdAsync(cartId)
                ?? throw new InvalidOperationException("Không tìm thấy giỏ hàng sau khi cập nhật");
        }
    }
}