namespace BookHub.Mvc.Models.User.Home;

public sealed class SuggestResponse
{
    public List<SuggestItem> Books { get; set; } = new();
    public List<SuggestItem> Genres { get; set; } = new();
    public List<SuggestItem> Publishers { get; set; } = new();
}