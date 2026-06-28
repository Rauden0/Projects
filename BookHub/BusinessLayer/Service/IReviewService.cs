using BusinessLayer.Dto.Review;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IReviewService
{
    public Task<Result<ReviewDto>> GetReview(int id);
    public Task<Result<List<ReviewDto>>> GetReviews(ODataQueryOptions<ReviewDto>? options);
    public Task<Result<ReviewDto>> AddReview(CreateReviewDto createReviewDto);
    public Task<Result<ReviewDto>> UpdateReview(int id, UpdateReviewDto updateReviewDto);
    public Task DeleteReview(int id);
    
    public Task<Result<List<ReviewDto>>> GetAllReviewsByBookId(int bookId);

}