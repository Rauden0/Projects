namespace BookHub.Mvc.Models.Admin;

public class AdminPublisherFormViewModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Name { get; set; } = string.Empty;
}