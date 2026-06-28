using BusinessLayer.Dto.Book;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Models.User;

public class HomeBookListItemViewModel
{
    public required BookDto Book { get; init; }
    public IEnumerable<SelectListItem> WishLists { get; set; } = new List<SelectListItem>();
    public string AuthorsText { get; init; } = "";
    public string GenresText { get; init; } = "";
    public string PublisherText { get; init; } = "";
}