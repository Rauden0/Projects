namespace BookHub.Mvc.Models.User.Home;

public sealed class SuggestItem
{
    public string Type { get; set; } = ""; // book/genre/publisher
    public int Id { get; set; }
    public string Label { get; set; } = "";
}