using BookHub.Mvc.Models.User.Review;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Review;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Models.User;

public class BookDetailViewModel
{
    public required BookDto Book { get; init; }
    public IEnumerable<SelectListItem> WishLists { get; set; } = new List<SelectListItem>();
    public string AuthorsText { get; init; } = "";
    public string GenresText { get; init; } = "";
    public string PublisherText { get; init; } = "";
    public List<ReviewDto> Reviews { get; set; } = new();
    
    public AddReviewViewModel NewReview { get; set; } = new();    
    
    public int CurrentUserId { get; set; }
    
    
}