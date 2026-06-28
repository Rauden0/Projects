using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Dto.Book;

public class RequestBookDto
{
    public List<int> AuthorIds { get; set; }
    public List<int> GenreIds { get; set; }
    public int PublisherId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public int StockQuantity { get; set; }
    public IFormFile? Image { get; set; }
    
    public int? PrimaryGenreId { get; set; }
}