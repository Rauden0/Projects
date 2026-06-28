using BusinessLayer.Dto.Review;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    public ReviewService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<ReviewDto>> GetReview(int id)
    {
        var review = await _uow.Reviews.GetByIdAsync(id);
        return review is not null
            ? ReviewMapper.ToDto(review)
            : new Result<ReviewDto>(new Exception($"Review {id} not found"));
    }

    public async Task<Result<List<ReviewDto>>> GetReviews(ODataQueryOptions<ReviewDto>? options)
    {
        return await ReviewMapper.ProjectToDto(_uow.Reviews.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<ReviewDto>> AddReview(CreateReviewDto createReviewDto)
    {
        var review = new Review
        {
            UserId = createReviewDto.UserId,
            BookId = createReviewDto.BookId,
            Rating = createReviewDto.Rating,
            Comment = createReviewDto.Comment
        };

        _uow.Reviews.Add(review);
        await _uow.SaveChangesAsync();
        return ReviewMapper.ToDto(review);
    }

    public async Task<Result<ReviewDto>> UpdateReview(int id, UpdateReviewDto updateReviewDto)
    {
        var review = await _uow.Reviews.GetByIdAsync(id);
        if (review is null)
        {
            return new Result<ReviewDto>(new Exception($"Review {id} not found"));
        }

        review.Rating = updateReviewDto.Rating;
        review.Comment = updateReviewDto.Comment;

        _uow.Reviews.Update(review);
        await _uow.SaveChangesAsync();
        return ReviewMapper.ToDto(review);
    }

    public async Task DeleteReview(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var review = await _uow.Reviews.GetByIdAsync(id);
            if (review != null)
                _uow.Reviews.Remove(review);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<Result<List<ReviewDto>>> GetAllReviewsByBookId(int bookId)
    {
        return await ReviewMapper.ProjectToDto(_uow.Reviews.Query().Where(r => r.BookId == bookId)).ToListAsync();
    }
}