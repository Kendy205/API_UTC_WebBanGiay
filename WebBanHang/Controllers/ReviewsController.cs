using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.IServices;
using WebBanHang.BLL.Services;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(IReviewService _reviewsService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            var reviews = await _reviewsService.GetAllAsync();
            return Ok(reviews);
        }
    }
}
