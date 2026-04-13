using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.IServices;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ICartItemService _cartItemService;
    private readonly IProductVariantService _variantService;

    public CartController(ICartService cartService, ICartItemService cartItemService, IProductVariantService variantService)
    {
        _cartService = cartService;
        _cartItemService = cartItemService;
        _variantService = variantService;
    }

    [HttpGet("mycart")]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized(ApiResponse<CartDto>.Failed("Vui lòng đăng nhập", 401));
        }

        var cart = await _cartService.GetCartByUserId(userId);
        if (cart == null)
        {
            return NotFound(ApiResponse<CartDto>.Failed("Giỏ hàng không tồn tại", 404));
        }

        return Ok(ApiResponse<CartDto>.Succeeded(cart, "Lấy giỏ hàng thành công"));
    }

    [HttpPost("addcart")]
    public async Task<IActionResult> AddItemToCart(AddCartItemRequest item)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized(ApiResponse<CartDto>.Failed("Vui lòng đăng nhập", 401));
        }

        var cart = await _cartService.GetCartByUserId(userId);
        var variant = await _variantService.GetByIdAsync(item.VariantId);
        if (variant == null)
        {
            return NotFound(ApiResponse<CartDto>.Failed("Biến thể không tồn tại", 404));
        }

        try
        {
            await _cartItemService.AddProductToCartAsync(cart.CartId, item.VariantId, item.Quantity);
            cart = await _cartService.GetCartByUserId(userId);
            return Ok(ApiResponse<CartDto>.Succeeded(cart, "Cập nhật cart thành công"));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(ApiResponse<CartDto>.Failed(e.Message, 400));
        }
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItemQuantity(long cartItemId, [FromBody] UpdateQuantityDto request)
    {
        try
        {
            var updatedCart = await _cartItemService.UpdateQuantityAsync(cartItemId, request.Quantity);
            return Ok(ApiResponse<CartDto>.Succeeded(updatedCart, "Cập nhật số lượng thành công"));
        }
        catch (ArgumentException e)
        {
            return BadRequest(ApiResponse<CartDto>.Failed(e.Message, 404));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(ApiResponse<CartDto>.Failed(e.Message, 404));
        }
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItemFromCart(long cartItemId)
    {
        try
        {
            var updatedCart = await _cartItemService.RemoveFromCartAsync(cartItemId);
            return Ok(ApiResponse<CartDto>.Succeeded(updatedCart, "Xóa sản phẩm thành công"));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(ApiResponse<CartDto>.Failed(e.Message));
        }
    }

    [HttpDelete("items")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized(ApiResponse<CartDto>.Failed("Hết hạn,Vui lòng đăng nhập", 401));
        }

        var cart = await _cartService.GetCartByUserId(userId);
        if (cart == null)
        {
            return NotFound(ApiResponse<CartDto>.Failed("Giỏ hàng không tồn tại", 404));
        }

        await _cartItemService.ClearCartAsync(cart.CartId);
        var emptyCart = await _cartService.GetByIdAsync(cart.CartId);
        return Ok(ApiResponse<CartDto>.Succeeded(emptyCart, "Đã xóa toàn bộ sản phẩm"));
    }

    public class AddCartItemRequest
    {
        public long VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddToCartRequest
    {
        public List<AddCartItemRequest>? Variants { get; set; }
    }

    public class UpdateQuantityDto
    {
        public int Quantity { get; set; }
    }
}
