using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.Review;

public class UpdateReviewDto
{
    public int BookId { get; set; }
    public int UserId { get; set; }
    public Rating Rating { get; set; }
    public required string Comment { get; set; }

}