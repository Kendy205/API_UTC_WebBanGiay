using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.IServices;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Common;


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

        // Admin endpoint: GET /api/Admin/reviews?page=1&pageSize=10&rating=5
        [HttpGet("/api/Admin/reviews")]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> GetAdminReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? rating = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, total) = await _reviewService.GetAdminReviewsAsync(page, pageSize, rating);

            var response = new
            {
                reviews = items,
                total = total
            };

            return Ok(response);
        }

        [HttpPut("/api/Admin/reviews/{id}/ispublic")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SetVisibility(long id, [FromBody] SetVisibilityRequest request)
        {
            if (request == null) return BadRequest();
            var updated = await _reviewService.SetVisibilityAsync(id, request.isPublic);
            if (!updated) return NotFound();
            return NoContent();
        }


        [HttpDelete("/api/Admin/reviews/{id}")]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _reviewService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _reviewService.DeleteAsync(id);
            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var item = await _reviewService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        // Admin: set visibility
        // PUT /api/Admin/reviews/{id}/visibility



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
        [Authorize] // keep or remove depending on whether you want anonymous reviews
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

            try
            {
                await _reviewService.AddAsync(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

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

        //[HttpDelete("{id:long}")]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    var existing = await _reviewService.GetByIdAsync(id);
        //    if (existing == null) return NotFound();
        //    await _reviewService.DeleteAsync(id);
        //    return NoContent();
        //}
        [HttpGet("product/{productId:long}")]
        public async Task<IActionResult> GetReviewByProductId(long productId)
        {
            try
            {
                var reviews = await _reviewService.GetByProductIdAsync(productId);
                if (reviews == null || !reviews.Any()) return NotFound(ApiResponse<string>.Failed("khong tim thay", 404));
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Succeeded(reviews));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failed(ex.Message));
            }
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
        // Request model for visibility toggle
        public class SetVisibilityRequest
        {
            public bool isPublic { get; set; }
        }

    }
}
