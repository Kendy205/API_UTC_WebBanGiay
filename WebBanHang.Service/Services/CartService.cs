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

        public async Task UpdateAsync(long userId, UpdateToCartRequest request)
        {
            //  Lấy cart 
            var cart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.UserId == userId);
            if (cart == null) throw new InvalidOperationException("Cart không tồn tại");

            //  Lấy toàn bộ cart items hiện tại 
            var currentItems = await _unitOfWork.CartItem.GetAllAsync(
                x => x.CartId == cart.CartId
            );

            //  Tạo danh sách VariantId từ request
            var requestVariantIds = request.variants.Select(v => v.VariantId).ToHashSet();

            //  XÓA: item có trong cart nhưng không có trong request
            var itemsToRemove = currentItems
                .Where(ci => !requestVariantIds.Contains(ci.VariantId))
                .ToList();

            foreach (var removeItem in itemsToRemove)
            {
                _unitOfWork.CartItem.Remove(removeItem);
            }

            //  XỬ LÝ TỪNG ITEM TRONG REQUEST
            foreach (var req in request.variants)
            {
                // Lấy variant + validate stock
                var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(
                    x => x.VariantId == req.VariantId,
                    includeProperties: "Product"
                );

                if (variant == null)
                    throw new InvalidOperationException("Biến thể không tồn tại");
                // Kiểm tra item đã tồn tại trong cart chưa
                var existingItem = currentItems.FirstOrDefault(x => x.VariantId == req.VariantId);

                if (existingItem != null)
                {
                    //  UPDATE
                    existingItem.Quantity = req.Quantity;
                    _unitOfWork.CartItem.Update(existingItem);
                }
                else
                {

                    var newItem = new CartItem
                    {
                        CartId = cart.CartId,
                        VariantId = req.VariantId,
                        Quantity = req.Quantity,
                        UnitPrice = (decimal)(variant.Product.SalePrice ?? variant.Product.BasePrice),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.CartItem.AddAsync(newItem);
                }
            }


            await _unitOfWork.SaveAsync();

        }
    }
}