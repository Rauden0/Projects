using System.Text.Json.Serialization;
using BusinessLayer.Dto.Genre;

namespace BusinessLayer.Dto.Book;

public class BookDto
{
    public int Id { get; set; }
    public List<int>? AuthorsIds { get; set; }
    
    public List<string>? AuthorsNames { get; set; }
    public List<int>? GenresIds { get; set; }
    public List<String>? GenresNames { get; set; }
    public int PublisherId { get; set; }
    
    public string? PublisherName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public int StockQuantity { get; set; }
    public List<int>? OrderItemIds { get; set; }
    public List<int>? WishlistItemIds { get; set; }
    public List<int>? ReviewsIds { get; set; }
    [JsonIgnore] 
    public string? ImagePath { get; set; }
    public string? ImageBase64 { get; set; }
    
    public GenreDto? PrimaryGenre {get ; set;  }
}