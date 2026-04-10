using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.BLL.IServices;

namespace WebBanHang.Controllers.ReviewController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _reviewService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var item = await _reviewService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // New endpoint: accepts payload with OrderItemId, ProductId, VariantId, userId, rating, ReviewContent
        // Example:
        // {
        //   "OrderItemId": "6",
        //   "ProductId": "16",
        //   "VariantId": "16",
        //   "userId": "123",         // optional if using JWT
        //   "rating": "5",
        //   "ReviewContent":"dsfgsdfgdf"
        // }
        [HttpPost("rating")]
        //[Authorize] // keep or remove depending on whether you want anonymous reviews
        public async Task<IActionResult> CreateDetailedRating([FromBody] CreateReviewRequest request)
        {
            if (request == null) return BadRequest();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // If JWT present, prefer authenticated user id
            if (User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
                if (idClaim != null && long.TryParse(idClaim.Value, out var userIdFromToken))
                {
                    request.UserId = userIdFromToken;
                }
            }

            if (request.OrderItemId == null || request.OrderItemId <= 0)
            {
                ModelState.AddModelError(nameof(request.OrderItemId), "OrderItemId is required and must be > 0.");
                return ValidationProblem(ModelState);
            }

            // Map request to ReviewDto (service will handle persistence)
            var dto = new ReviewDto
            {
                OrderItemId = request.OrderItemId.Value,
                UserId = request.UserId ?? 0, // if 0, service or DB constraints should validate
                Rating = request.Rating,
                ReviewTitle = request.ReviewTitle,
                ReviewContent = request.ReviewContent
            };

            await _reviewService.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.ReviewId }, dto);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] ReviewDto dto)
        {
            var existing = await _reviewService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _reviewService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _reviewService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _reviewService.DeleteAsync(id);
            return NoContent();
        }

        // Local request model to accept ProductId and VariantId in payload without changing existing DTO/entity.
        public class CreateReviewRequest
        {
            [Required]
            public long? OrderItemId { get; set; }

            // Optional fields -- kept for client convenience
            //public long? ProductId { get; set; }
            //public long? VariantId { get; set; }

            // Optional: client may pass userId; if JWT auth enabled the token value will override this
            public long? UserId { get; set; }

            [Required]
            [Range(1, 5)]
            public short Rating { get; set; }
            [MaxLength(255)] // Thêm Title theo đúng DB 
            public string? ReviewTitle { get; set; }

            public string? ReviewContent { get; set; }
        }
    }
}
