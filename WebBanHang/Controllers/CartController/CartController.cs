using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;

[Route("api/[controller]")]
[ApiController]
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
     
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0) 
            return base.Unauthorized(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Failed("Vui lòng đăng nhập", 401));

        var cart = await _cartService.GetActiveCartByUserIdAsync(userId);
        if (cart == null) 
            return base.NotFound(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Failed("Giỏ hàng không tồn tại", 404));

        return base.Ok(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Succeeded(cart, "Lấy giỏ hàng thành công"));
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> UpdateItemToCart([FromBody] UpdateToCartRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0) return base.Unauthorized(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Failed("Vui lòng đăng nhập", 401));

        try
        {
            await _cartService.UpdateAsync(userId, request);
            var cart = await _cartService.GetOrCreateCartForUserAsync(userId);
            return base.Ok(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Succeeded(cart, "Cập nhật cart thành công"));
        }
        catch (InvalidOperationException e)
        {            
            return base.BadRequest(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Failed(e.Message, 400));
        }
    }


    [HttpDelete("clear")]
    [Authorize]
    public async Task<IActionResult> ClearCart()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0) 
            return base.Unauthorized(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Failed("Hết hạn,Vui lòng đăng nhập", 401));
        var cart = await _cartService.GetActiveCartByUserIdAsync(userId);

        await _cartItemService.ClearCartAsync(cart.CartId);
        // Sau khi xóa, client có thể gọi lại GET /me để lấy cart rỗng, hoặc trả về cart rỗng luôn
        var emptyCart = await _cartService.GetByIdAsync(cart.CartId);
        return base.Ok(ApiResponse<WebBanHang.Service.DTOs.Model.CartDto>.Succeeded(emptyCart, "Đã xóa toàn bộ sản phẩm"));
    }

    
}