using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.Review;

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public Rating Rating { get; set; }
    public string Comment { get; set; } = null!;
    
    public DateTime CreatedAt { get; set;  }

    public string? UserDisplayName { get; set; } = string.Empty;


}