using BusinessLayer.Dto.Review;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class ReviewMapper
{
    public static ReviewDto ToDto(Review entity) =>
        EntityMapper.ToDto<Review, ReviewDto>(entity);

    public static IQueryable<ReviewDto> ProjectToDto(IQueryable<Review> query) =>
        EntityMapper.ProjectToDto<Review, ReviewDto>(query);
}