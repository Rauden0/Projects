using System.ComponentModel.DataAnnotations;

namespace BookHub.Mvc.Models.Admin;

public class AdminAuthorFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}