
using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Enums;

namespace BookHub.Mvc.Models.User.Review;

public class AddReviewViewModel
{
    public int BookId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5 stars.")]
    public Rating Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}