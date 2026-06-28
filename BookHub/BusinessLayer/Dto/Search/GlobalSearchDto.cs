namespace BusinessLayer.Dto.GlobalSearch;

public class GlobalSearchDto
{
    public List<BookSearchItemDto> Books { get; set; } = new();
    public List<GenreSearchItemDto> Genres { get; set; } = new();
    public List<PublisherSearchItemDto> Publishers { get; set; } = new();
}

public record BookSearchItemDto(int Id, string Name, string? ImagePath);
public record GenreSearchItemDto(int Id, string Name);
public record PublisherSearchItemDto(int Id, string Name);