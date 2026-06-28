using BusinessLayer.Dto.Review;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReviewDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetReviews([FromQuery] ODataQueryOptions<ReviewDto>? options)
    {
        var reviews = await _reviewService.GetReviews(options);
        return reviews.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReview(int id)
    {
        var review = await _reviewService.GetReview(id);
        return review.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }


    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
    {
        var review = await _reviewService.AddReview(dto);
        return review.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        var review = await _reviewService.UpdateReview(id, dto);
        return review.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    [HttpDelete("{id:int}")]
    public async Task DeleteReview(int id)
    {
        await _reviewService.DeleteReview(id);
    }
}