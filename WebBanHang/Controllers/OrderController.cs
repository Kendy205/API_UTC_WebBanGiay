using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.Claims;
using WebBanHang.BLL.IServices;
using WebBanHang.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController(IOrderService _orderService, ICartService _cartService) : ControllerBase
    {
        //[HttpPost]
        //public async Task<IActionResult> CreateOrderFromCart()
        //{
        //    long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //    if(userId == null)
        //    {
        //        return BadRequest(ApiResponse<IEnumerable<OrderDto>>.Failed("UserId is null"));
        //    }
        //    var cart = await _cartService.GetCartByUserId(userId);
        //    long cartId = cart.CartId;  
        //    if (cart == null)
        //    {
        //        return BadRequest(ApiResponse<IEnumerable<OrderDto>>.Failed("Cart is null"));
        //    }
        //    var orders = await _orderService.CreateOrderFromCart(cartId);

        //    return Ok(ApiResponse<IEnumerable<OrderDto>>.Succeeded(orders));
        //}
     }
}
